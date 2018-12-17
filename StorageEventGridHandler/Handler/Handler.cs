using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Handler
{
	public static class Handler
	{
		[FunctionName("Handler")]
		public static async Task<HttpResponseMessage> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestMessage req,
			ILogger log)
		{
			string response = string.Empty;
			string requestContent = string.Empty;

			if (req?.Content == null)
				log.LogInformation("Received events: null");
			else
			{
				requestContent = await req.Content.ReadAsStringAsync();
				log.LogInformation($"Received events: {requestContent}");
			}

			if (string.IsNullOrWhiteSpace(requestContent))
			{
				log.LogInformation("Request content was empty.");
			}
			else
			{
				log.LogInformation("Prepping deserialize");
				EventGridSubscriber eventGridSubscriber = new EventGridSubscriber();
				EventGridEvent[] eventGridEvents = null;

				try
				{
					eventGridEvents = eventGridSubscriber.DeserializeEventGridEvents(requestContent);
					log.LogInformation("eventGridEvents null? " + (eventGridEvents == null));
				}
				catch (Exception ex)
				{
					log.LogError("eventGridEvents error: " + ex.Message);

				}

				if (eventGridEvents != null)
				{
					foreach (EventGridEvent eventGridEvent in eventGridEvents)
					{
						log.LogInformation("eventGridEvent null? " + (eventGridEvent == null));

						if (eventGridEvent.Data is SubscriptionValidationEventData)
						{
							var eventData = (SubscriptionValidationEventData)eventGridEvent.Data;

							log.LogInformation($"Got SubscriptionValidation event data, validationCode: {eventData.ValidationCode},  validationUrl: {eventData.ValidationUrl}, topic: {eventGridEvent.Topic}");

							// Do any additional validation (as required) such as validating that the Azure resource ID of the topic matches
							// the expected topic and then return back the below response
							var responseData = new SubscriptionValidationResponse()
							{
								ValidationResponse = eventData.ValidationCode
							};

							return req.CreateResponse(HttpStatusCode.OK, responseData);
						}
						else
						{
							log.LogInformation("Data " + eventGridEvent.Data.ToString());
						}
					}
				}
			}

			return req.CreateResponse(HttpStatusCode.OK, response);
		}
	}
}
