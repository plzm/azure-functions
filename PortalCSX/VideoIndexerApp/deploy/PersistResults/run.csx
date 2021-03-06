#r "Newtonsoft.Json"

using System.IO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using pelazem.azure.storage;
using pelazem.azure.cognitive.videoindexer;

public static async Task Run(HttpRequest req, ILogger log)
{
    // App settings
	string storageConnString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
	string policyName = Environment.GetEnvironmentVariable("PolicyName");
	string containerNameVideoResults = Environment.GetEnvironmentVariable("ContainerNameVideoResults");

	string videoIndexerApiAccountId = Environment.GetEnvironmentVariable("VideoIndexerApiAccountId");
	string videoIndexerApiUrl = Environment.GetEnvironmentVariable("VideoIndexerApiUrl");
	string videoIndexerApiKey = Environment.GetEnvironmentVariable("VideoIndexerApiKey");
	string videoIndexerApiAzureRegion = Environment.GetEnvironmentVariable("VideoIndexerApiAzureRegion");

	log.LogInformation(nameof(storageConnString) + "=" + storageConnString);
	log.LogInformation(nameof(policyName) + "=" + policyName);
	log.LogInformation(nameof(containerNameVideoResults) + "=" + containerNameVideoResults);

	log.LogInformation(nameof(videoIndexerApiAccountId) + "=" + videoIndexerApiAccountId);
	log.LogInformation(nameof(videoIndexerApiUrl) + "=" + videoIndexerApiUrl);
	log.LogInformation(nameof(videoIndexerApiKey) + "=" + videoIndexerApiKey);
	log.LogInformation(nameof(videoIndexerApiAzureRegion) + "=" + videoIndexerApiAzureRegion);

    // Request querystring params
    string videoId = req.Query["id"];
    string state = req.Query["state"];

    log.LogInformation($"{nameof(videoId)} = {videoId}");
    log.LogInformation($"{nameof(state)} = {state}");
    
    // Do we need to continue?
    if (state != "Processed")
        return;
    
    // Get video info from the Video Indexer API and serialize it to JSON
    JsonSerializerSettings settings = new JsonSerializerSettings() { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Include };

    VideoIndexerService videoIndexerService = new VideoIndexerService(videoIndexerApiAccountId, videoIndexerApiUrl, videoIndexerApiKey, videoIndexerApiAzureRegion);

    VideoIndexerVideo video = await videoIndexerService.GetVideo(videoId, true);

    string videoName = Path.GetFileNameWithoutExtension(video.Name);
    log.LogInformation($"{nameof(videoName)} = {videoName}");
    
    string videoJson = JsonConvert.SerializeObject(video, settings);

    // Write the JSON to storage
    ServiceClient storageClient = new ServiceClient();
	CloudStorageAccount storageAccount = storageClient.GetStorageAccount(storageConnString);
    string sasUrl = await storageClient.GetBlobSAPUrlFromBlobUrlAsync(storageAccount, containerNameVideoResults, url, policyName);

    bool result = await storageClient.UploadStringAsync(config, videoJson, videoName + ".txt");
    log.LogInformation($"{nameof(result)} = {result.ToString()}");
}
