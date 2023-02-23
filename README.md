# ZoomNet.TokenRepositories.Azure

Save Zoom's OAuth token information in Azure blob storage

```csharp
var azureStorageAccountName = "...your storage account name...";
var azureStorageAccountKey = "...your storage account key...";
var azureStorageConnectionString = $"DefaultEndpointsProtocol=https;AccountName={azureStorageAccountName};AccountKey={azureStorageAccountKey}";

var containerName = "zoomoauthtokenstorage";
var blobName = "ZoomOAuth.txt";

var tokenRepository = new ZoomNet.TokenRepositories.Azure.AzureBlobStorageTokenRepository(azureStorageConnectionString, containerName, blobName);
var connectionInfo = OAuthConnectionInfo.ForServerToServer(clientId, clientSecret, accountId, new[] { 0 }, tokenRepository);
var zoomClient = new ZoomClient(connectionInfo);
```