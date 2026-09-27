using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;

namespace JamfDotNet.Core.Kiota;

/// <summary>
/// <see cref="IRequestAdapter"/> decorator that remaps <see cref="ApiException"/> to <see cref="JamfApiException"/>.
/// </summary>
public sealed class JamfRemappingRequestAdapter : IRequestAdapter
{
    private readonly IRequestAdapter _inner;

    /// <summary>
    /// Creates a remapping adapter around an existing Kiota adapter.
    /// </summary>
    /// <param name="inner">Inner request adapter.</param>
    public JamfRemappingRequestAdapter(IRequestAdapter inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    /// <inheritdoc />
    public string? BaseUrl
    {
        get => _inner.BaseUrl;
        set => _inner.BaseUrl = value;
    }

    /// <inheritdoc />
    public ISerializationWriterFactory SerializationWriterFactory => _inner.SerializationWriterFactory;

    /// <inheritdoc />
    public void EnableBackingStore(IBackingStoreFactory backingStoreFactory) =>
        _inner.EnableBackingStore(backingStoreFactory);

    /// <inheritdoc />
    public Task<T?> ConvertToNativeRequestAsync<T>(RequestInformation requestInfo, CancellationToken cancellationToken = default) =>
        MapAsync(() => _inner.ConvertToNativeRequestAsync<T>(requestInfo, cancellationToken));

    /// <inheritdoc />
    public Task<ModelType?> SendAsync<ModelType>(
        RequestInformation requestInfo,
        ParsableFactory<ModelType> factory,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default,
        CancellationToken cancellationToken = default)
        where ModelType : IParsable =>
        MapAsync(() => _inner.SendAsync(requestInfo, factory, errorMapping, cancellationToken));

    /// <inheritdoc />
    public Task<IEnumerable<ModelType>?> SendCollectionAsync<ModelType>(
        RequestInformation requestInfo,
        ParsableFactory<ModelType> factory,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default,
        CancellationToken cancellationToken = default)
        where ModelType : IParsable =>
        MapAsync(() => _inner.SendCollectionAsync(requestInfo, factory, errorMapping, cancellationToken));

    /// <inheritdoc />
    public Task<ModelType?> SendPrimitiveAsync<ModelType>(
        RequestInformation requestInfo,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default,
        CancellationToken cancellationToken = default) =>
        MapAsync(() => _inner.SendPrimitiveAsync<ModelType>(requestInfo, errorMapping, cancellationToken));

    /// <inheritdoc />
    public Task<IEnumerable<ModelType>?> SendPrimitiveCollectionAsync<ModelType>(
        RequestInformation requestInfo,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default,
        CancellationToken cancellationToken = default) =>
        MapAsync(() => _inner.SendPrimitiveCollectionAsync<ModelType>(requestInfo, errorMapping, cancellationToken));

    /// <inheritdoc />
    public Task SendNoContentAsync(
        RequestInformation requestInfo,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default,
        CancellationToken cancellationToken = default) =>
        MapAsync(() => _inner.SendNoContentAsync(requestInfo, errorMapping, cancellationToken));

    /// <summary>
    /// Wraps <paramref name="inner"/> when remapping is enabled; otherwise returns <paramref name="inner"/>.
    /// </summary>
    /// <param name="inner">Inner adapter.</param>
    /// <param name="remapKiotaExceptions">Whether to remap exceptions.</param>
    /// <returns>Possibly wrapped adapter.</returns>
    public static IRequestAdapter MaybeWrap(IRequestAdapter inner, bool remapKiotaExceptions) =>
        remapKiotaExceptions ? new JamfRemappingRequestAdapter(inner) : inner;

    private static async Task<T> MapAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (ApiException ex)
        {
            throw JamfApiException.FromApiException(ex);
        }
    }

    private static async Task MapAsync(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (ApiException ex)
        {
            throw JamfApiException.FromApiException(ex);
        }
    }
}
