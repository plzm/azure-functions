using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BingMapsRESTToolkit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AddressLookup
{
	public class BingMapsApiClient
	{
		private string ApiKey { get; set; }

		private BingMapsApiClient() { }

		public BingMapsApiClient(string apiKey)
		{
			this.ApiKey = apiKey;
		}

		public async Task<AddressLookup.Location> ProcessAddress(string address, string culture = "en-US")
		{
			AddressLookup.Location result = null;

			var request = new GeocodeRequest();

			request.BingMapsKey = this.ApiKey;
			request.Culture = culture;
			request.IncludeIso2 = true;
			request.IncludeNeighborhood = true;
			request.UserIp = "127.0.0.1";

			request.Query = address;

			var response = await request.Execute();

			if
			(
				response != null &&
				response.ResourceSets != null &&
				response.ResourceSets.Length > 0 &&
				response.ResourceSets[0].Resources != null &&
				response.ResourceSets[0].Resources.Length > 0
			)
			{
				var bingResult = response.ResourceSets[0].Resources[0] as BingMapsRESTToolkit.Location;

				result = GetLocation(culture, address, bingResult);
			}

			return result;
		}

		private AddressLookup.Location GetLocation(string culture, string rawAddress, BingMapsRESTToolkit.Location bingResult)
		{
			AddressLookup.Location result = new AddressLookup.Location();

			result.Culture = culture;
			result.RawAddress = rawAddress;

			result.BoundingBox = bingResult.BoundingBox;
			result.Confidence = bingResult.Confidence;

			Coordinate coordinate = bingResult.Point.GetCoordinate();
			result.Latitude = coordinate.Latitude;
			result.Longitude = coordinate.Longitude;

			result.Address = new AddressLookup.Address();
			result.Address.AddressLine = bingResult.Address.AddressLine;
			result.Address.AdminDistrict = bingResult.Address.AdminDistrict;
			result.Address.AdminDistrict2 = bingResult.Address.AdminDistrict2;
			result.Address.CountryRegion = bingResult.Address.CountryRegion;
			result.Address.CountryRegionIso2 = bingResult.Address.CountryRegionIso2;
			result.Address.FormattedAddress = bingResult.Address.FormattedAddress;
			result.Address.Landmark = bingResult.Address.Landmark;
			result.Address.Locality = bingResult.Address.Locality;
			result.Address.Neighborhood = bingResult.Address.Neighborhood;
			result.Address.PostalCode = bingResult.Address.PostalCode;

			return result;
		}
	}
}
