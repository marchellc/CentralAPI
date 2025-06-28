using CentralAPI.ClientPlugin.Databases;
using CentralAPI.ClientPlugin.PlayerProfiles.Internal;
using CentralAPI.SharedLib.PlayerProfiles;
using NetworkLib;

namespace CentralAPI.ClientPlugin.PlayerProfiles.Properties;

/// <summary>
/// Base class for custom properties that implements some custom things like dirtiness management.
/// </summary>
public abstract class CustomPlayerProfileProperty<T> : PlayerProfilePropertyBase
{
    private T value;
    private bool isAssigned;

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
}