using CentralAPI.ClientPlugin.Core; 

using CentralAPI.SharedLib.Requests; 

using LabExtended.API;
using LabExtended.Extensions;

using LabExtended.Core;
using LabExtended.Core.Pooling.Pools; 

using NetworkLib;
using NetworkLib.Pools;
using NetworkLib.Interfaces;

using NetworkClient = CentralAPI.ClientPlugin.Network.NetworkClient;

using StringExtensions = CommonLib.Extensions.StringExtensions;

namespace CentralAPI;

/// <summary>
/// Represents a connected central server.
/// </summary>
public class ScpInstance : NetworkComponent
{
    private static ushort requestId = 1;
    
    private static Dictionary<ushort, Action<NetworkReader, NetworkWriter>> requestHandlers;
    private static Dictionary<ushort, Action<NetworkReader>> responseHandlers;

    private static Dictionary<Type, Func<INetworkMessage, bool>> messageHandlers;
    
    /// <summary>
    /// Gets called when the entity spawn gets confirmed.
    /// </summary>
    public override void Start()
    {
        base.Start();

        requestHandlers = DictionaryPool<ushort, Action<NetworkReader, NetworkWriter>>.Shared.Rent();
        responseHandlers = DictionaryPool<ushort, Action<NetworkReader>>.Shared.Rent();
        
        messageHandlers = DictionaryPool<Type, Func<INetworkMessage, bool>>.Shared.Rent();

        requestId = 1;
        
        HandleRequest("ClientIdentify", (_, writer) =>
        {
            writer.WriteUShort(ExServer.Port);
            
            writer.WriteString(ExServer.Name.RemoveHtmlTags());
            writer.WriteString(CentralPlugin.Config.ServerAlias ?? string.Empty);
        });
        
        NetworkClient.scp = this;
        NetworkClient.OnReady();
        
        ApiLog.Info("Network Client", "Started!");
    }

    /// <summary>
    /// Gets called when the entity gets destroyed.
    /// </summary>
    public override void Stop()
    {
        base.Stop();
        
        if (requestHandlers != null)
            DictionaryPool<ushort, Action<NetworkReader, NetworkWriter>>.Shared.Return(requestHandlers);
        
        if (responseHandlers != null)
            DictionaryPool<ushort, Action<NetworkReader>>.Shared.Return(responseHandlers);
        
        if (messageHandlers != null)
            DictionaryPool<Type, Func<INetworkMessage, bool>>.Shared.Return(messageHandlers);

        requestHandlers = null;
        responseHandlers = null;

        messageHandlers = null;

        NetworkClient.OnDestroyed();
        NetworkClient.scp = null;
        
        ApiLog.Info("Network Client", "Stopped!");
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
        
        requestHandlers[StringExtensions.GetShortCode(requestType)] = handler;
        
        ApiLog.Debug("Network Client", $"Registered handler for request &3{requestType}&r: &6{handler.Method?.GetMemberName() ?? "null"}&r");
    }

    /// <summary>
    /// Registers a message handler.
    /// </summary>
    /// <param name="type">The type of message to handle.</param>
    /// <param name="handler">The delegate used to handle the message.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public void HandleMessage(Type type, Func<INetworkMessage, bool> handler)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        if (!typeof(INetworkMessage).IsAssignableFrom(type))
            throw new Exception($"Type '{type.FullName}' is not an INetworkMessage");

        messageHandlers[type] = handler;
        
        ApiLog.Debug("Network Client", $"Registered message handler for type &3{type.FullName}&r: &6{handler.Method?.GetMemberName() ?? "null"}&r");
    }

    /// <summary>
    /// Registers a message handler.
    /// </summary>
    /// <param name="handler">The delegate used to handle the message.</param>
    /// <typeparam name="T">The type of message to handle.</typeparam>
    public void HandleMessage<T>(Func<T, bool> handler) where T : INetworkMessage
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));
        
        messageHandlers[typeof(T)] = message =>
        {
            if (message is not T castMessage)
                return false;

            return handler(castMessage);
        };
        
        ApiLog.Debug("Network Client", $"Registered message handler for type &3{typeof(T).FullName}&r: &6{handler.Method?.GetMemberName() ?? "null"}&r");
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
        
        requestHandlers?.Remove(StringExtensions.GetShortCode(requestType));
    }

    /// <summary>
    /// Removes a message handler.
    /// </summary>
    /// <param name="messageType">The type of message.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void RemoveMessageHandler(Type messageType)
    {
        if (messageType is null)
            throw new ArgumentNullException(nameof(messageType));

        messageHandlers?.Remove(messageType);
    }

    /// <summary>
    /// Removes a message handler.
    /// </summary>
    /// <typeparam name="T">The type of message.</typeparam>
    public void RemoveMessageHandler<T>() where T : INetworkMessage
    {
        messageHandlers?.Remove(typeof(T));
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

        return StringExtensions.GetShortCode(requestType);
    }
    
    /// <summary>
    /// Sends a request to the server.
    /// </summary>
    /// <param name="requestType">The type of the request (must have a handler on the server).</param>
    /// <param name="requestWriter">The request data writer.</param>
    /// <param name="responseHandler">The response data reader.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Request(string requestType, Action<NetworkWriter>? requestWriter = null, Action<NetworkReader>? responseHandler = null)
    {
        if (string.IsNullOrEmpty(requestType))
            throw new ArgumentNullException(nameof(requestType));

        if (responseHandler != null)
        {
            var requestId = ScpInstance.requestId++;

            responseHandlers[requestId] = responseHandler;
            
            ApiLog.Debug("Network Client", $"Sending request with response ({responseHandler.Method?.GetMemberName() ?? "null"}): {requestType} ({requestId})");
            
            Send(new RequestMessage(requestId, StringExtensions.GetShortCode(requestType), requestWriter is null
                        ? null
                        : NetworkDataPool.GetWriter(requestWriter)));
        }
        else
        {
            ApiLog.Debug("Network Client", $"Sending request without response: {requestType}");
            
            Send(new RequestMessage(0, StringExtensions.GetShortCode(requestType), requestWriter is null
                        ? null
                        : NetworkDataPool.GetWriter(requestWriter)));
        }
    }
    
    /// <summary>
    /// Sends a request to the server.
    /// </summary>
    /// <param name="requestType">The type of the request (must have a handler on the server).</param>
    /// <param name="requestWriter">The request data writer.</param>
    /// <param name="responseHandler">The response data reader.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Request(ushort requestType, Action<NetworkWriter>? requestWriter = null, Action<NetworkReader>? responseHandler = null)
    {
        if (responseHandler != null)
        {
            var requestId = ScpInstance.requestId++;

            responseHandlers[requestId] = responseHandler;
            
            ApiLog.Debug("Network Client", $"Sending request with response ({responseHandler.Method?.GetMemberName() ?? "null"}): {requestType} ({requestId})");
            
            Send(new RequestMessage(requestId, requestType, requestWriter is null 
                    ? null
                    : NetworkDataPool.GetWriter(requestWriter)));
        }
        else
        {
            ApiLog.Debug("Network Client", $"Sending request without response: {requestType}");
            
            Send(new RequestMessage(0, requestType, requestWriter is null
                        ? null
                        : NetworkDataPool.GetWriter(requestWriter)));
        }
    }

    /// <summary>
    /// Processes network messages.
    /// </summary>
    /// <param name="message">The message to process.</param>
    /// <returns>true if the message was processed</returns>
    public override bool Process(INetworkMessage message)
    {
        if (base.Process(message))
            return true;

        if (message is RequestMessage requestMessage)
        {
            ApiLog.Debug("Network Client", $"Received request Type={requestMessage.RequestType}; Id={requestMessage.RequestId}; Size={requestMessage.Reader?.Count ?? -1}");
            
            if (!requestHandlers.TryGetValue(requestMessage.RequestType, out var handler))
            {
                ApiLog.Warn("Network Client", $"Received request without a registered handler: &3{requestMessage.RequestType}&r");
                return true;
            }
            
            ApiLog.Debug("Network Client", $"Request Handler: {handler.Method?.GetMemberName() ?? "null"}");

            if (requestMessage.RequestId == 0)
            {
                handler.InvokeSafe(requestMessage.Reader, null);
                
                requestMessage.Reader?.Return();
                
                ApiLog.Debug("Network Client", $"Handled without response");
                return true;
            }
            
            var writer = NetworkDataPool.GetWriter();
            
            handler.InvokeSafe(requestMessage.Reader, writer);
            
            Send(new ResponseMessage(requestMessage.RequestId, writer));
            
            requestMessage.Reader?.Return();
            
            ApiLog.Debug("Network Client", $"Handled with response");
            return true;
        }

        if (message is ResponseMessage responseMessage)
        {
            ApiLog.Debug("Network Client", $"Received response Id={responseMessage.RequestId}; Reader={responseMessage.Reader?.Count ?? -1}");
            
            if (!responseHandlers.TryGetValue(responseMessage.RequestId, out var handler))
            {
                ApiLog.Warn("Network Client", $"Received response without a registered handler: &3{responseMessage.RequestId}&r");
                
                responseMessage.Reader?.Return();
                return true;
            }
            
            responseHandlers.Remove(responseMessage.RequestId);
            
            handler.InvokeSafe(responseMessage.Reader);
            
            responseMessage.Reader?.Return();
            
            ApiLog.Debug("Network Client", $"Handled response");
            return true;
        }

        if (messageHandlers.TryGetValue(message.GetType(), out var messageHandler))
            return messageHandler.InvokeSafe(message);

        return false;
    }
}