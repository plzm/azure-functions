using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace pelazem
{
    public static class HelloWorld
    {
        [FunctionName("HelloWorld")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
			log.LogInformation("C# HTTP trigger function processed a request.");

			foreach (var r in req.Query)
				log.LogInformation($"{r.Key} = {r.Value}");

			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			string data = JsonConvert.DeserializeObject(requestBody).ToString();

			log.LogInformation(data);

			return req.Query.Count > 0
				? (ActionResult)new OkObjectResult($"Thanks for that query string!")
				: new BadRequestObjectResult("Please pass something on the query string!");
        }
    }
}
