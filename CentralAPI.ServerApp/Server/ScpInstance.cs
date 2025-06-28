using System.Collections.Concurrent;
using CentralAPI.ServerApp.Databases;
using CentralAPI.SharedLib.Requests; 

using CommonLib;
using CommonLib.Extensions;

using NetworkLib;
using NetworkLib.Pools;
using NetworkLib.Interfaces; 

namespace CentralAPI;

using ServerApp.Server;

/// <summary>
/// Represents a connected SCP server.
/// </summary>
public class ScpInstance : NetworkComponent
{
    /// <summary>
    /// A list of registered delegates used to handle messages.
    /// </summary>
    public static volatile ConcurrentDictionary<Type, Func<ScpInstance, INetworkMessage, bool>> MessageHandlers = new();
    
    /// <summary>
    /// Registers a message handler.
    /// </summary>
    /// <param name="type">The type of message to handle.</param>
    /// <param name="handler">The delegate used to handle the message.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static void HandleMessage(Type type, Func<ScpInstance, INetworkMessage, bool> handler)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        if (!typeof(INetworkMessage).IsAssignableFrom(type))
            throw new Exception($"Type '{type.FullName}' is not an INetworkMessage");

        MessageHandlers.TryRemove(type, out _);
        MessageHandlers.TryAdd(type, handler);
        
        CommonLog.Debug("Scp Manager", $"Registered message handler '{type.FullName}': {handler.Method?.ToString() ?? "null"}");
    }

    /// <summary>
    /// Registers a message handler.
    /// </summary>
    /// <param name="handler">The delegate used to handle the message.</param>
    /// <typeparam name="T">The type of message to handle.</typeparam>
    public static void HandleMessage<T>(Func<ScpInstance, T, bool> handler) where T : INetworkMessage
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));
        
        Func<ScpInstance, INetworkMessage, bool> customHandler = (server, message) =>
        {
            if (message is not T castMessage)
                return false;

            return handler(server, castMessage);
        };

        MessageHandlers.TryRemove(typeof(T), out _);
        MessageHandlers.TryAdd(typeof(T), customHandler);
        
        CommonLog.Debug("Scp Manager", $"Registered message handler '{typeof(T).FullName}': {handler.Method?.ToString() ?? "null"}");
    }
    
    /// <summary>
    /// Removes a message handler.
    /// </summary>
    /// <param name="messageType">The type of message.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void RemoveMessageHandler(Type messageType)
    {
        if (messageType is null)
            throw new ArgumentNullException(nameof(messageType));

        MessageHandlers.TryRemove(messageType, out _);
    }
    
    /// <summary>
    /// Removes a message handler.
    /// </summary>
    /// <typeparam name="T">The type of message.</typeparam>
    public static void RemoveMessageHandler<T>() where T : INetworkMessage
    {
        MessageHandlers.TryRemove(typeof(T), out _);
    }
    
    private volatile bool isTerminated;
    private volatile bool isIdentified;

    private volatile ushort port;

    private volatile string name;
    private volatile string alias;
    
    private volatile ushort requestId = 1;

    private volatile ConcurrentDictionary<ushort, Action<NetworkReader, NetworkWriter>> requestHandlers = new();
    private  volatile ConcurrentDictionary<ushort, Action<NetworkReader>> responseHandlers = new();
    
    /// <summary>
    /// Whether or not this server has been terminated.
    /// </summary>
    public bool IsTerminated => isTerminated;
    
    /// <summary>
    /// Whether or not this server has identified itself.
    /// </summary>
    public bool IsIdentified => isIdentified;
    
    /// <summary>
    /// Gets the server's server-list port.
    /// </summary>
    public ushort Port => port;
    
    /// <summary>
    /// Gets the server's name.
    /// </summary>
    public string Name => name;
    
    /// <summary>
    /// Gets the server's alias.
    /// </summary>
    public string Alias => alias;

    /// <summary>
    /// Gets called once the component is started.
    /// </summary>
    public override void Start()
    {
        base.Start();

        requestId = 1;

        Request("ClientIdentify", null, reader =>
        {
            port = reader.ReadUShort();

            name = reader.ReadString();
            alias = reader.ReadString();

            isIdentified = true;
            
            ScpManager.SetIdentified(this);
        });
        
        DatabaseRequests.Register(this);
    }

    /// <summary>
    /// Gets called once the entity is destroyed.
    /// </summary>
    public override void Stop()
    {
        base.Stop();

        isTerminated = true;
        
        requestHandlers.Clear();
        responseHandlers.Clear();
        
        ScpManager.SetTerminated(this);
    }
    
    /// <summary>
    /// Registers a request handler.
    /// </summary>
    /// <param name="requestType">The request type.</param>
    /// <param name="handler">The request handler delegate.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void HandleRequest(string requestType, Action<NetworkReader, NetworkWriter> handler)
    {
        if (string.IsNullOrEmpty(requestType))
            throw new ArgumentNullException(nameof(requestType));

        if (handler is null)
            throw new ArgumentNullException(nameof(handler));
        
        requestHandlers.TryAdd(requestType.GetShortCode(), handler);
        
        CommonLog.Debug("Scp Manager", $"Registered request handler '{requestType}': {handler.Method?.ToString() ?? "null"}");
    }
    
    /// <summary>
    /// Gets a stable short code of a string, used for identifying request types.
    /// </summary>
    /// <param name="requestType">The request type.</param>
    /// <returns>The stable short code.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public ushort GetRequestType(string requestType)
    {
        if (string.IsNullOrEmpty(requestType))
            throw new ArgumentNullException(nameof(requestType));

        return requestType.GetShortCode();
    }
    
    /// <summary>
    /// Removes a request handler.
    /// </summary>
    /// <param name="requestType"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void RemoveRequestHandler(string requestType)
    {
        if (string.IsNullOrEmpty(requestType))
            throw new ArgumentNullException(nameof(requestType));
        
        requestHandlers?.TryRemove(requestType.GetShortCode(), out _);
    }
    
    /// <summary>
    /// Sends a request to the client.
    /// </summary>
    /// <param name="requestType">The type of the request (must have a handler on the client).</param>
    /// <param name="requestWriter">The request data writer.</param>
    /// <param name="responseHandler">The response data reader.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Request(string requestType, Action<NetworkWriter>? requestWriter = null, Action<NetworkReader>? responseHandler = null)
    {
        if (string.IsNullOrEmpty(requestType))
            throw new ArgumentNullException(nameof(requestType));

        if (responseHandler != null)
        {
            var requestId = (ushort)(this.requestId + 1);

            this.requestId = requestId;

            responseHandlers.TryAdd(requestId, responseHandler);
            
            CommonLog.Debug("Scp Manager", $"Sending request with response ('{responseHandler.Method?.ToString() ?? "null"}'): {requestType} ({requestId})");
            
            Send(new RequestMessage(requestId, requestType.GetShortCode(), requestWriter is null 
                    ? null
                    : NetworkDataPool.GetWriter(requestWriter)));
        }
        else
        {
            CommonLog.Debug("Scp Manager", $"Sending request without response: {requestType}");
            
            Send(new RequestMessage(0, requestType.GetShortCode(), requestWriter is null
                    ? null
                    : NetworkDataPool.GetWriter(requestWriter)));
        }
    }
    
    /// <summary>
    /// Sends a request to the client.
    /// </summary>
    /// <param name="requestType">The type of the request (must have a handler on the client).</param>
    /// <param name="requestWriter">The request data writer.</param>
    /// <param name="responseHandler">The response data reader.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Request(ushort requestType, Action<NetworkWriter>? requestWriter = null, Action<NetworkReader>? responseHandler = null)
    {
        if (responseHandler != null)
        {
            var requestId = (ushort)(this.requestId + 1);

            this.requestId = requestId;

            responseHandlers.TryAdd(requestId, responseHandler);
            
            CommonLog.Debug("Scp Manager", $"Sending request with response ('{responseHandler.Method?.ToString() ?? "null"}'): {requestType} ({requestId})");
            
            Send(new RequestMessage(requestId, requestType, requestWriter is null 
                ? null
                : NetworkDataPool.GetWriter(requestWriter)));
        }
        else
        {
            CommonLog.Debug("Scp Manager", $"Sending request without response: {requestType}");
            
            Send(new RequestMessage(0, requestType, requestWriter is null 
                ? null
                : NetworkDataPool.GetWriter(requestWriter)));
        }
    }

    public override bool Process(INetworkMessage message)
    {
        if (base.Process(message))
            return true;

        try
        {
            if (message is RequestMessage requestMessage)
            {
                CommonLog.Debug("Scp Manager",
                    $"Processing request Id={requestMessage.RequestId}; Type={requestMessage.RequestType}");

                if (!requestHandlers.TryGetValue(requestMessage.RequestType, out var handler))
                {
                    CommonLog.Warn("Scp Client",
                        $"Received request without a registered handler: {requestMessage.RequestType}");
                    return true;
                }

                CommonLog.Debug("Scp Manager", $"Response Handler: {handler.Method?.ToString() ?? "null"}");

                if (requestMessage.RequestId == 0)
                {
                    CommonLog.Debug("Scp Manager", $"Processing without response");

                    handler(requestMessage.Reader, null);

                    requestMessage.Reader?.Return();

                    CommonLog.Debug("Scp Manager", $"Processed without response");
                    return true;
                }

                CommonLog.Debug("Scp Manager", $"Processing with response");

                var writer = NetworkDataPool.GetWriter();

                handler(requestMessage.Reader, writer);

                Send(new ResponseMessage(requestMessage.RequestId, writer));

                requestMessage.Reader?.Return();

                CommonLog.Debug("Scp Manager", $"Processed with response");
                return true;
            }

            if (message is ResponseMessage responseMessage)
            {
                CommonLog.Debug("Scp Manager", $"Processing response Id={responseMessage.RequestId}");

                if (!responseHandlers.TryRemove(responseMessage.RequestId, out var handler))
                {
                    CommonLog.Warn("Scp Client",
                        $"Received response without a registered handler: {responseMessage.RequestId}");

                    responseMessage.Reader?.Return();
                    return true;
                }

                CommonLog.Debug("Scp Manager", $"Response Handler: {handler.Method?.ToString() ?? "null"}");

                handler(responseMessage.Reader);

                responseMessage.Reader?.Return();

                CommonLog.Debug("Scp Manager", $"Processed response");
                return true;
            }

            if (MessageHandlers.TryGetValue(message.GetType(), out var messageHandler))
                return messageHandler(this, message);
        }
        catch (Exception ex)
        {
            CommonLog.Error("Scp Manager", ex);
        }

        return false;
    }
}