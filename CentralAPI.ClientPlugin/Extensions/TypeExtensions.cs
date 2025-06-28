using LabExtended.Extensions;

namespace CentralAPI.ClientPlugin.Extensions;

/// <summary>
/// Extensions targeting Type reflection.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Whether or not a specific type overrides the <see cref="object.ToString"/> method.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>true if the method is overriden by the specified type</returns>
    public static bool OverridesToString(this Type type)
    {
        if (type is null)
            return false;

        var toStringMethod = type.FindMethod(x => x.Name == "ToString");

        if (toStringMethod?.DeclaringType is null || toStringMethod.DeclaringType != type)
            return false;

        return true;
    }
}