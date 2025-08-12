using System.Threading.Tasks;

namespace lib.Services;

/// <summary>
/// Abstraction over a browser-like LocalStorage key-value store.
/// </summary>
/// <remarks>
/// Implementations are expected to provide persistent, per-origin (or app) storage of simple key/value pairs,
/// similar to the <c>window.localStorage</c> API in web browsers.
/// Values may be serialized (e.g., JSON) to store non-string types.
/// Keys are case-sensitive; missing keys should be handled gracefully.
/// </remarks>
public interface ILocalStorage
{
    /// <summary>
    /// Stores a value under the specified key.
    /// </summary>
    /// <typeparam name="T">Type of the value to store. Implementations may serialize it.</typeparam>
    /// <param name="key">The key to associate with the stored value. Must be non-null/non-empty.</param>
    /// <param name="value">The value to store. May be <c>null</c> for reference types.</param>
    /// <returns>A task that completes when the value has been persisted.</returns>
    /// <remarks>
    /// If a value already exists for the key, it should be overwritten.
    /// </remarks>
    Task SetItemAsync<T>(string key, T value);

    /// <summary>
    /// Retrieves the value previously stored under the specified key.
    /// </summary>
    /// <typeparam name="T">Expected type of the stored value. Implementations may deserialize to this type.</typeparam>
    /// <param name="key">The key to look up. Must be non-null/non-empty.</param>
    /// <returns>
    /// A task that resolves to the stored value if present; otherwise <c>default(T)</c>.
    /// </returns>
    /// <remarks>
    /// If the stored data cannot be converted to <typeparamref name="T"/>, implementations may throw
    /// a serialization or format exception.
    /// </remarks>
    Task<T> GetItemAsync<T>(string key);

    /// <summary>
    /// Removes the value stored under the specified key and returns it.
    /// </summary>
    /// <typeparam name="T">Expected type of the stored value to return.</typeparam>
    /// <param name="key">The key to remove. Must be non-null/non-empty.</param>
    /// <returns>
    /// A task that resolves to the removed value if it existed; otherwise <c>default(T)</c>.
    /// </returns>
    /// <remarks>
    /// If no value exists for the key, implementations should complete successfully and return <c>default</c>.
    /// </remarks>
    Task<T> RemoveKeyAsync<T>(string key);

    /// <summary>
    /// Removes the value stored under the specified key.
    /// </summary>
    /// <param name="key">The key to remove. Must be non-null/non-empty.</param>
    /// <returns>A task that completes when the key has been removed (or if it did not exist).</returns>
    Task RemoveKeyAsync(string key);
}