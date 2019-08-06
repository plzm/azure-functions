using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CosmosDbUpsertTester
{
	public static class CosmosDbUpserter
	{
		[FunctionName("CosmosDbUpserter")]
		public static async Task Run
		(
			[BlobTrigger("azfn/{name}")]Stream triggerBlob,
			string name,
			[CosmosDB(
				databaseName: "rdb1",
				collectionName: "docs",
				ConnectionStringSetting = "CosmosDBConnection")]
				IAsyncCollector<dynamic> outputDocuments,
			ILogger log
		)
		{
			log.LogInformation($"CosmosDbUpserter trigger blob. Name: {name}, Size: {triggerBlob.Length} B");

			string fileContent = string.Empty;

			using (var rdr = new StreamReader(triggerBlob))
			{
				fileContent = await rdr.ReadToEndAsync();
			}

			dynamic doc = JsonConvert.DeserializeObject(fileContent);

			await outputDocuments.AddAsync(doc);
			log.LogInformation($"Updated document: {doc}");
		}
	}
}
