using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace HelloWorld
{
	public static class HelloWorld
	{
		[FunctionName("HelloWorld")]
		public static async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
			ILogger log)
		{
			log.LogInformation("C# HTTP trigger function processed a request.");

			if (req?.Query != null)
			{
				log.LogInformation($"Query string has {req.Query.Count} values");

				foreach (var r in req.Query)
					log.LogInformation($"{r.Key} = {r.Value}");
			}

			if (req?.Body != null)
			{
				log.LogInformation("Request body was not null");

				string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

				log.LogInformation("Deserializing Request body");

				dynamic data = JsonConvert.DeserializeObject(requestBody);

				if (data != null)
				{
					var dataD = data as IDictionary<string, object>;

					if (dataD != null)
					{
						foreach (var key in dataD.Keys)
							log.LogInformation($"{key} = {dataD[key]}");
					}
					else
						log.LogInformation("Body payload could not be deserialized");
				}
				else
					log.LogInformation($"{nameof(data)} is null");
			}
			else
				log.LogInformation("Request body was null");


			return (req?.Query != null && req.Query.Count > 0)
				? (ActionResult)new OkObjectResult($"Thanks for that query string!")
				: new BadRequestObjectResult("Please pass something on the query string!");
		}
	}
}
