using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace AddressLookup
{
	[DataContract]
	public class Location
	{
		private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

		[DataMember(Name = "culture", EmitDefaultValue = false)]
		public string Culture { get; set; }

		[DataMember(Name = "rawAddress", EmitDefaultValue = false)]
		public string RawAddress { get; set; }

		[DataMember(Name = "latitude", EmitDefaultValue = false)]
		public double Latitude { get; set; }

		[DataMember(Name = "longitude", EmitDefaultValue = false)]
		public double Longitude { get; set; }

		[DataMember(Name = "confidence", EmitDefaultValue = false)]
		public string Confidence { get; set; }

		[DataMember(Name = "boundingBox", EmitDefaultValue = false)]
		public double[] BoundingBox { get; set; }

		[DataMember(Name = "address", EmitDefaultValue = false)]
		public Address Address { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented, jsonSerializerSettings);
		}
	}
}
