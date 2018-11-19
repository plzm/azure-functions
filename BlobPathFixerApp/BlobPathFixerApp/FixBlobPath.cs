using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BlobPathFixerApp
{
	public static class FixBlobPath
	{
		[FunctionName("FixBlobPath")]
		public static async Task Run([BlobTrigger("files-raw/{name}")]Stream inputBlob, string name, ILogger log, ExecutionContext context)
		{
			log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {inputBlob.Length} Bytes");

			var storageConnString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

			// URL-decode the blob path and replace spaces with underscores
			string targetBlobPath = "xlsm/" + WebUtility.UrlDecode(name).Replace(" ", "_");
			log.LogInformation($"TargetBlob Path: {targetBlobPath}");

			CloudStorageAccount sa = CloudStorageAccount.Parse(storageConnString);

			CloudBlobClient blobClient = sa.CreateCloudBlobClient();

			CloudBlobContainer targetContainer = blobClient.GetContainerReference("files-stage");

			if (!(await targetContainer.ExistsAsync()))
				await targetContainer.CreateIfNotExistsAsync();

			CloudBlockBlob targetBlob = targetContainer.GetBlockBlobReference(targetBlobPath);

			inputBlob.Position = 0;
			byte[] bytes;

			using (var memoryStream = new MemoryStream())
			{
				await inputBlob.CopyToAsync(memoryStream);

				bytes = memoryStream.ToArray();
			}

			await targetBlob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
		}
	}
}
