using CentralAPI.ClientPlugin.Databases;
using CentralAPI.ClientPlugin.PlayerProfiles.Internal;
using CentralAPI.SharedLib.PlayerProfiles;
using NetworkLib;

namespace CentralAPI.ClientPlugin.PlayerProfiles.Properties;

/// <summary>
/// A property that uses a database wrapper to write / read it's data (DOES NOT INTERACT WITH THE DATABASE).
/// </summary>
public class DatabasePlayerProfileProperty<T> : PlayerProfilePropertyBase
{
    private T value;
    private bool isAssigned;
    
    /// <summary>
    /// Gets the wrapper used to write / read data.
    /// </summary>
    public DatabaseWrapper<T> Wrapper { get; private set; }

    /// <inheritdoc cref="PlayerProfilePropertyBase.IsAssigned"/>
    public override bool IsAssigned => isAssigned;

    /// <inheritdoc cref="PlayerProfilePropertyBase.ObjectValue"/>
    public override object ObjectValue
    {
        get => this.value;
        set => this.value = value is null ? default : (T)value;
    }

    /// <summary>
    /// Gets or sets the value of the property.
    /// </summary>
    public T Value
    {
        get => this.value;
        set
        {
            if (this.value is null && value is null)
                return;
            
            this.value = value;

            IsDirty = true;
            isAssigned = true;
        }
    }
    
    /// <inheritdoc cref="PlayerProfilePropertyBase.Initialize"/>
    public override void Initialize()
    {
        if (!DatabaseDirector.TryGetWrapper<T>(out var wrapper))
            throw new Exception($"Could not get DatabaseWrapper for type {typeof(T).FullName}");
        
        Wrapper = wrapper;
    }

    /// <inheritdoc cref="PlayerProfilePropertyBase.Read"/>
    public override void Read(NetworkReader reader, bool isNew)
    {
        Wrapper.Read(reader, ref value);
    }

    /// <inheritdoc cref="PlayerProfilePropertyBase.Write"/>
    public override void Write(NetworkWriter writer)
    {
        Wrapper.Write(writer, ref value);
    }
}