using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Picton;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ZoomNet.TokenRepositories.Azure
{
	/// <summary>
	/// Store token in Azure blob storage.
	/// </summary>
	internal class AzureBlobStorageTokenManagementStrategy : ITokenRepository
	{
		private readonly BlobClient _blob;

		private string _leaseId = null;

		public AzureBlobStorageTokenManagementStrategy(string connectionString, string containerName, string blobName)
		{
			var blobContainer = new BlobContainerClient(connectionString, containerName);
			blobContainer.CreateIfNotExists(PublicAccessType.None);

			_blob = blobContainer.GetBlobClient(blobName);
		}

		/// <inheritdoc/>
		public async Task<(string RefreshToken, string AccessToken, DateTime TokenExpiration, int TokenIndex)> GetTokenInfoAsync(CancellationToken cancellationToken = default)
		{
			string refreshToken = null;
			string accessToken = null;
			DateTime tokenExpiration = DateTime.MinValue;
			int tokenIndex = 0;

			var blobExistsResponse = await _blob.ExistsAsync(cancellationToken).ConfigureAwait(false);
			if (!blobExistsResponse.Value) return (refreshToken, accessToken, tokenExpiration, tokenIndex);

			var downloadResponse = await _blob.DownloadAsync(cancellationToken).ConfigureAwait(false);
			using (var reader = new StreamReader(downloadResponse.Value.Content, true))
			{
				while (!reader.EndOfStream)
				{
					var line = await reader.ReadLineAsync().ConfigureAwait(false);
					var parts = line.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);

					if (parts.Length >= 2)
					{
						switch (parts[0])
						{
							case "refreshToken":
								refreshToken = parts[1];
								break;
							case "accessToken":
								accessToken = parts[1];
								break;
							case "tokenExpiration":
								tokenExpiration = new DateTime(long.Parse(parts[1]));
								break;
							case "tokenIndex":
								tokenIndex = int.Parse(parts[1]);
								break;
						}
					}
				}
			}

			return (refreshToken, accessToken, tokenExpiration, tokenIndex);
		}

		/// <inheritdoc/>
		public async Task AcquireLeaseAsync(CancellationToken cancellationToken = default)
		{
			_leaseId = await _blob.TryAcquireLeaseAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
			if (string.IsNullOrEmpty(_leaseId)) throw new TimeoutException("Unable to acquire an exclusive lease for the purpose of updating token information");
		}

		/// <inheritdoc/>
		public async Task SaveTokenInfoAsync(string refreshToken, string accessToken, DateTime tokenExpiration, int tokenIndex, CancellationToken cancellationToken = default)
		{
			var stream = new MemoryStream();
			var sw = new StreamWriter(stream);

			sw.WriteLine($"refreshToken: {refreshToken}");
			sw.WriteLine($"accessToken: {accessToken}");
			sw.WriteLine($"tokenExpiration: {tokenExpiration.Ticks}");
			sw.WriteLine($"tokenIndex: {tokenIndex}");

			await _blob.UploadStreamAsync(stream, _leaseId, "text/plain", null, null, cancellationToken).ConfigureAwait(false);
		}

		/// <inheritdoc/>
		public Task ReleaseLeaseAsync(CancellationToken cancellationToken = default)
		{
			return _blob.ReleaseLeaseAsync(_leaseId, cancellationToken);
		}
	}
}
