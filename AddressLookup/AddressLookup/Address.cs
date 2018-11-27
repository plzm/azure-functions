using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace AddressLookup
{
	/// <summary>
	/// Based on BingMapsRESTToolkit Address model but made neutral for re-use with other address APIs
	/// See https://github.com/Microsoft/BingMapsRESTToolkit/blob/master/Source/Models/ResponseModels/Address.cs
	/// </summary>
	[DataContract]
	public class Address
	{
		private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

		/// <summary>
		/// The official street line of an address relative to the area, as specified by the Locality, or PostalCode, properties. 
		/// Typical use of this element would be to provide a street address or any official address.
		/// </summary>
		[DataMember(Name = "addressLine", EmitDefaultValue = false)]
		public string AddressLine { get; set; }

		/// <summary>
		/// A string specifying the subdivision name in the country or region for an address. This element is typically treated as 
		/// the first order administrative subdivision, but in some cases it is the second, third, or fourth order subdivision in a 
		/// country, dependency, or region. 
		/// </summary>
		[DataMember(Name = "adminDistrict", EmitDefaultValue = false)]
		public string AdminDistrict { get; set; }

		/// <summary>
		/// A string specifying the subdivision name in the country or region for an address. This element is used when there is 
		/// another level of subdivision information for a location, such as the county.
		/// </summary>
		[DataMember(Name = "adminDistrict2", EmitDefaultValue = false)]
		public string AdminDistrict2 { get; set; }

		/// <summary>
		/// A string specifying the country or region name of an address.
		/// </summary>
		[DataMember(Name = "countryRegion", EmitDefaultValue = false)]
		public string CountryRegion { get; set; }

		/// <summary>
		/// A string specifying the populated place for the address. This typically refers to a city, but may refer to a suburb or a 
		/// neighborhood in certain countries.
		/// </summary>
		[DataMember(Name = "locality", EmitDefaultValue = false)]
		public string Locality { get; set; }

		/// <summary>
		/// A string specifying the post code, postal code, or ZIP Code of an address.
		/// </summary>
		[DataMember(Name = "postalCode", EmitDefaultValue = false)]
		public string PostalCode { get; set; }

		/// <summary>
		/// A string specifying the two-letter ISO country code.
		/// </summary>
		[DataMember(Name = "countryRegionIso2", EmitDefaultValue = false)]
		public string CountryRegionIso2 { get; set; }

		/// <summary>
		/// A string specifying the complete address. This address may not include the country or region.
		/// </summary>
		[DataMember(Name = "formattedAddress", EmitDefaultValue = false)]
		public string FormattedAddress { get; set; }

		/// <summary>
		/// A string specifying the neighborhood for an address. You must specify includeNeighborhood=1 in your request to return 
		/// the neighborhood.
		/// </summary>
		[DataMember(Name = "neighborhood", EmitDefaultValue = false)]
		public string Neighborhood { get; set; }

		/// <summary>
		/// A string specifying the name of the landmark when there is a landmark associated with an address.
		/// </summary>
		[DataMember(Name = "landmark", EmitDefaultValue = false)]
		public string Landmark { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented, jsonSerializerSettings);
		}
	}
}
