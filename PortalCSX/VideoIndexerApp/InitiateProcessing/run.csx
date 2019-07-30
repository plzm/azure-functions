#r "Microsoft.Azure.EventGrid"
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using pelazem.azure.storage;
using pelazem.azure.cognitive.videoindexer;

public static async Task Run(EventGridEvent eventGridEvent, ILogger log)
{
	string storageConnString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
	string policyName = Environment.GetEnvironmentVariable("PolicyName");
	string containerNameVideoRaw = Environment.GetEnvironmentVariable("ContainerNameVideoRaw");

	string videoIndexerApiAccountId = Environment.GetEnvironmentVariable("VideoIndexerApiAccountId");
	string videoIndexerApiUrl = Environment.GetEnvironmentVariable("VideoIndexerApiUrl");
	string videoIndexerApiKey = Environment.GetEnvironmentVariable("VideoIndexerApiKey");
	string videoIndexerApiAzureRegion = Environment.GetEnvironmentVariable("VideoIndexerApiAzureRegion");

	log.LogInformation(nameof(storageConnString) + "=" + storageConnString);
	log.LogInformation(nameof(policyName) + "=" + policyName);
	log.LogInformation(nameof(containerNameVideoRaw) + "=" + containerNameVideoRaw);

	log.LogInformation(nameof(videoIndexerApiAccountId) + "=" + videoIndexerApiAccountId);
	log.LogInformation(nameof(videoIndexerApiUrl) + "=" + videoIndexerApiUrl);
	log.LogInformation(nameof(videoIndexerApiKey) + "=" + videoIndexerApiKey);
	log.LogInformation(nameof(videoIndexerApiAzureRegion) + "=" + videoIndexerApiAzureRegion);

    log.LogInformation(nameof(eventGridEvent.Topic) + "=" + eventGridEvent.Topic);
    log.LogInformation(nameof(eventGridEvent.Subject) + "=" + eventGridEvent.Subject);
    log.LogInformation(nameof(eventGridEvent.EventType) + "=" + eventGridEvent.EventType);
    log.LogInformation(nameof(eventGridEvent.EventTime) + "=" + eventGridEvent.EventTime);

	// Reference: https://docs.microsoft.com/en-us/azure/event-grid/event-schema-blob-storage
	dynamic eventPayload = JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());

	string api = eventPayload.api;
	string blobType = eventPayload.blobType;
	string contentType = eventPayload.contentType;
	string url = eventPayload.url;
	int contentLength = eventPayload.contentLength;

	log.LogInformation(nameof(api) + "=" + api);
	log.LogInformation(nameof(blobType) + "=" + blobType);
	log.LogInformation(nameof(contentType) + "=" + contentType);
	log.LogInformation(nameof(contentLength) + "=" + contentLength.ToString());
	log.LogInformation(nameof(url) + "=" + url);

    bool proceed = ((api == "CopyBlob" || api == "PutBlob" || api == "PutBlockBlobList") && contentLength > 0);
    if (!proceed)
    {
        log.LogInformation("Exiting");
        return;
    }

    string callbackUrl = "https://[CALLBACKFUNCTION].azurewebsites.net/api/PersistResults";

    Uri uri = new Uri(url);
    string videoName = Path.GetFileName(uri.AbsolutePath);

    // Get Shared Access Policy URL for the blob
    ServiceClient storageClient = new ServiceClient();
	CloudStorageAccount storageAccount = storageClient.GetStorageAccount(storageConnString);
    string sasUrl = await storageClient.GetBlobSAPUrlFromBlobUrlAsync(storageAccount, containerNameVideoRaw, url, policyName);

	log.LogInformation(nameof(sasUrl) + "=" + sasUrl);

    // Initiate upload to the Video Indexer Cognitive Service
    VideoIndexerService videoIndexerService = new VideoIndexerService(videoIndexerApiAccountId, videoIndexerApiUrl, videoIndexerApiKey, videoIndexerApiAzureRegion);
    VideoIndexerVideoInput videoInput = new VideoIndexerVideoInput(){ Name = videoName, UrlOriginal = url, UrlVisibleToVideoIndexer = sasUrl };
    string videoId = await videoIndexerService.UploadVideo(videoInput, string.Empty, callbackUrl);

	log.LogInformation(nameof(videoId) + "=" + videoId);
}
