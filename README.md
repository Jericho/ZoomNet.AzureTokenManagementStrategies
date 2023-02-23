# ZoomNet.TokenRepositories.Azure

Save Zoom's OAuth token information in Azure blob storage

```csharp
// Replace the following values with your actual Azure storage account information
var azureStorageAccountName = "...your storage account name...";
var azureStorageAccountKey = "...your storage account key...";

// Customize the following values
var containerName = "zoomoauthtokenstorage";
var blobName = "ZoomOAuth.txt";

// Setup the ZoomNet connection info
var azureStorageConnectionString = $"DefaultEndpointsProtocol=https;AccountName={azureStorageAccountName};AccountKey={azureStorageAccountKey}";
var tokenRepository = new ZoomNet.TokenRepositories.Azure.AzureBlobStorageTokenRepository(azureStorageConnectionString, containerName, blobName);
var connectionInfo = OAuthConnectionInfo.ForServerToServer(clientId, clientSecret, accountId, new[] { 0 }, tokenRepository);
var zoomClient = new ZoomClient(connectionInfo);
```