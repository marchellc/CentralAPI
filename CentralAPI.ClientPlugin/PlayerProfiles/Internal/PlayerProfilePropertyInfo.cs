namespace CentralAPI.ClientPlugin.PlayerProfiles.Internal;

/// <summary>
/// Contains info about a registered player property.
/// </summary>
public struct PlayerProfilePropertyInfo
{
    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the type of the class.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Creates a new <see cref="PlayerProfilePropertyInfo"/> instance.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="type">The type of the value.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public PlayerProfilePropertyInfo(string name, Type type)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        
        Name = name;
        Type = type;
    }

    /// <summary>
    /// Creates an instance of the specified property.
    /// </summary>
    /// <returns>The created property instance.</returns>
    /// <exception cref="Exception"></exception>
    public PlayerProfilePropertyBase Create()
    {
        if (string.IsNullOrEmpty(Name))
            throw new Exception("Name is undefined!");

        if (Activator.CreateInstance(Type) is not PlayerProfilePropertyBase property)
            throw new Exception("Could not instantiate property");

        property.Name = Name;
        return property;
    }
}