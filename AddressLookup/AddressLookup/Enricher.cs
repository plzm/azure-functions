using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using BingMapsRESTToolkit;
using Newtonsoft.Json;
using System;

namespace AddressLookup
{
	public static class Enricher
	{
		[FunctionName("Enricher")]
		public static async Task Run(
			[CosmosDBTrigger
			(
				databaseName: "addresses",
				collectionName: "addresses-raw",
				ConnectionStringSetting = "CosmosDBConnection",
				LeaseCollectionName = "leases",
				CreateLeaseCollectionIfNotExists = true,
				StartFromBeginning = true,
				PreferredLocations = "eastus"
			)]
			IReadOnlyList<Document> inputDocuments,
			[CosmosDB(
				databaseName: "addresses",
				collectionName: "addresses-stage",
				ConnectionStringSetting = "CosmosDBConnection")]
				IAsyncCollector<Output> outputDocuments,
			ILogger log
		)
		{
			if (inputDocuments != null && inputDocuments.Count > 0)
			{
				log.LogInformation("Documents modified " + inputDocuments.Count);
				log.LogInformation("First document Id " + inputDocuments[0].Id);
			}

			string bingMapsApiKey = Environment.GetEnvironmentVariable("BingMapsAPIKey");
			BingMapsApiClient apiClient = new BingMapsApiClient(bingMapsApiKey);

			foreach (Document inputDocument in inputDocuments)
			{
				string id = inputDocument.GetPropertyValue<string>("id");
				string sourcePath = inputDocument.GetPropertyValue<string>("sourcePath");
				string addressText = inputDocument.GetPropertyValue<string>("addressText");

				Output output = new Output()
				{
					Id = id,
					SourcePath = sourcePath,
					AddressText = addressText,
					Location = await apiClient.ProcessAddress(addressText)
				};

				await outputDocuments.AddAsync(output);
			}
		}
	}
}
