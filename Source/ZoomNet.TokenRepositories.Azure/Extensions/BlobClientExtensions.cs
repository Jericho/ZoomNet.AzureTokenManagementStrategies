namespace ZoomNet.TokenRepositories.Azure.Extensions
{
	/// <summary>
	/// Contains extension methods for the <see cref="BlobClient"/> class.
	/// </summary>
	//internal static class BlobClientExtensions
	//{
	//	private static readonly Stream EmptyStream = new MemoryStream();
	//	private static readonly IDictionary<string, string> EmptyDictionary = new Dictionary<string, string>();

	//	#region PUBLIC EXTENSION METHODS

	//	/// <summary>
	//	/// Attempt to acquire a lease asynchronously.
	//	/// </summary>
	//	/// <param name="blob">The blob.</param>
	//	/// <param name="leaseTime">The lease duration. If specified, this value must be between 15 and 60 seconds.</param>
	//	/// <param name="maxAttempts">The maximum number of attempts. If specified, this value must be between 1 and 10.</param>
	//	/// <param name="cancellationToken">The cancellation token.</param>
	//	/// <returns>The lease Id.</returns>
	//	public static async Task<string> TryAcquireLeaseAsync(this BlobClient blob, TimeSpan? leaseTime = null, int maxAttempts = 1, CancellationToken cancellationToken = default)
	//	{
	//		if (blob == null) throw new ArgumentNullException(nameof(blob));
	//		if (maxAttempts < 1 || maxAttempts > 10)
	//		{
	//			throw new ArgumentOutOfRangeException(nameof(maxAttempts), "The number of attempts must be between 1 and 10");
	//		}

	//		var leaseId = (string)null;
	//		for (var attempts = 0; attempts < maxAttempts; attempts++)
	//		{
	//			try
	//			{
	//				leaseId = await blob.AcquireLeaseAsync(leaseTime, cancellationToken).ConfigureAwait(false);
	//				if (!string.IsNullOrEmpty(leaseId)) break;
	//			}
	//			catch (RequestFailedException e) when (e.ErrorCode == "LeaseAlreadyPresent")
	//			{
	//				if (attempts < maxAttempts - 1)
	//				{
	//					await Task.Delay(500).ConfigureAwait(false);    // Make sure we don't retry too quickly
	//				}
	//			}
	//		}

	//		return leaseId;
	//	}

	//	/// <summary>
	//	/// Acquire a lease asynchronously.
	//	/// </summary>
	//	/// <param name="blob">The blob.</param>
	//	/// <param name="leaseTime">The lease duration. If specified, this value must be between 15 and 60 seconds.</param>
	//	/// <param name="cancellationToken">The cancellation token.</param>
	//	/// <returns>The lease Id.</returns>
	//	public static async Task<string> AcquireLeaseAsync(this BlobClient blob, TimeSpan? leaseTime = null, CancellationToken cancellationToken = default)
	//	{
	//		// From: https://msdn.microsoft.com/en-us/library/azure/ee691972.aspx
	//		// The Lease Blob operation establishes and manages a lock on a blob for write and delete operations.
	//		// The lock duration can be 15 to 60 seconds, or can be infinite.
	//		if (leaseTime.HasValue && (leaseTime.Value < TimeSpan.FromSeconds(15) || leaseTime.Value > TimeSpan.FromSeconds(60)))
	//		{
	//			throw new ArgumentOutOfRangeException(nameof(leaseTime), "Lease duration must be between 15 and 60 seconds");
	//		}

	//		var leaseClient = new BlobLeaseClient(blob, null);
	//		var defaultLeaseTime = TimeSpan.FromSeconds(15);

	//		try
	//		{
	//			// Optimistically try to acquire the lease. The blob may not yet
	//			// exist. If it doesn't we handle the 404, create it, and retry below
	//			var response = await leaseClient.AcquireAsync(leaseTime.GetValueOrDefault(defaultLeaseTime), null, cancellationToken).ConfigureAwait(false);
	//			return response.Value.LeaseId;
	//		}
	//		catch (RequestFailedException e) when (e.ErrorCode == "BlobNotFound")
	//		{
	//			await blob.CreateAsync(null, null, true, cancellationToken: cancellationToken).ConfigureAwait(false);
	//			var response = await leaseClient.AcquireAsync(leaseTime.GetValueOrDefault(defaultLeaseTime), null, cancellationToken).ConfigureAwait(false);

	//			return response.Value.LeaseId;
	//		}
	//	}

	//	/// <summary>
	//	/// Release a lease.
	//	/// </summary>
	//	/// <param name="blob">The blob.</param>
	//	/// <param name="leaseId">The lease Id.</param>
	//	/// <param name="cancellationToken">The cancellation token.</param>
	//	/// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
	//	public static Task ReleaseLeaseAsync(this BlobClient blob, string leaseId = null, CancellationToken cancellationToken = default)
	//	{
	//		if (blob == null) throw new ArgumentNullException(nameof(blob));

	//		var leaseClient = new BlobLeaseClient(blob, leaseId);
	//		return leaseClient.ReleaseAsync(null, cancellationToken);
	//	}

	//	/// <summary>
	//	/// Upload the content of a stream to a blob. If the blog already exist, it will be overwritten.
	//	/// </summary>
	//	/// <param name="blob">The blob.</param>
	//	/// <param name="content">The stream.</param>
	//	/// <param name="leaseId">The lease identifier.</param>
	//	/// <param name="mimeType">The MIME type.</param>
	//	/// <param name="cacheControl">The directives for caching mechanisms.</param>
	//	/// <param name="contentEncoding">The content encoding.</param>
	//	/// <param name="cancellationToken">The cancellation token.</param>
	//	/// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
	//	public static async Task UploadStreamAsync(this BlobClient blob, Stream content, string leaseId = null, string mimeType = null, string cacheControl = null, string contentEncoding = null, CancellationToken cancellationToken = default)
	//	{
	//		if (blob == null) throw new ArgumentNullException(nameof(blob));
	//		if (content == null) throw new ArgumentNullException(nameof(content));

	//		content.Position = 0; // Rewind the stream. IMPORTANT!

	//		BlobRequestConditions requestConditions = null;
	//		if (!string.IsNullOrEmpty(leaseId))
	//		{
	//			requestConditions = new BlobRequestConditions { LeaseId = leaseId };
	//		}

	//		try
	//		{
	//			BlobProperties properties = await blob.GetPropertiesAsync(requestConditions, cancellationToken).ConfigureAwait(false);

	//			var headers = new BlobHttpHeaders()
	//			{
	//				ContentType = mimeType ?? properties?.ContentType,
	//				CacheControl = cacheControl ?? properties?.CacheControl,
	//				ContentEncoding = contentEncoding ?? properties?.ContentEncoding
	//			};

	//			await blob.UploadAsync(content ?? BlobClientExtensions.EmptyStream, headers, properties?.Metadata, requestConditions, null, null, default, cancellationToken).ConfigureAwait(false);
	//		}
	//		catch (RequestFailedException e) when (e.ErrorCode == "BlobNotFound")
	//		{
	//			await blob.CreateAsync(content, BlobClientExtensions.EmptyDictionary, true, mimeType, cacheControl, contentEncoding, cancellationToken).ConfigureAwait(false);
	//		}
	//	}

	//	/// <summary>
	//	/// Creates a blob.
	//	/// </summary>
	//	/// <param name="blob">The blob.</param>
	//	/// <param name="content">The content.</param>
	//	/// <param name="metadata">Custom metadata to set for this blob.</param>
	//	/// <param name="overwriteIfExists">Indicates if existing blob should be overwritten.</param>
	//	/// <param name="mimeType">The MIME content type of the blob.</param>
	//	/// <param name="cacheControl">Specify directives for caching mechanisms.</param>
	//	/// <param name="contentEncoding">Specifies which content encoding has been applied to the blob.</param>
	//	/// <param name="cancellationToken">The cancellation token.</param>
	//	/// <returns>A <see cref="Azure.Response{T}">response</see> describing the state of the blob.</returns>
	//	public static async Task CreateAsync(this BlobClient blob, Stream content = null, IDictionary<string, string> metadata = null, bool overwriteIfExists = true, string mimeType = null, string cacheControl = null, string contentEncoding = null, CancellationToken cancellationToken = default)
	//	{
	//		if (blob == null) throw new ArgumentNullException(nameof(blob));

	//		var headers = new BlobHttpHeaders()
	//		{
	//			CacheControl = cacheControl,
	//			ContentEncoding = contentEncoding,
	//			ContentType = mimeType
	//		};

	//		BlobRequestConditions requestConditions = null;
	//		if (!overwriteIfExists)
	//		{
	//			requestConditions = new BlobRequestConditions
	//			{
	//				IfNoneMatch = ETag.All
	//			};
	//		}

	//		await blob.UploadAsync(content ?? BlobClientExtensions.EmptyStream, headers, metadata, requestConditions, null, null, default, cancellationToken).ConfigureAwait(false);
	//	}

	//	#endregion
	//}
}
