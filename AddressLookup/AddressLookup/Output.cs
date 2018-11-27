using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace AddressLookup
{
	[DataContract]
	public class Output
	{
		private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

		[DataMember(Name = "id", EmitDefaultValue = false)]
		public string Id { get; set; }

		[DataMember(Name = "sourcePath", EmitDefaultValue = false)]
		public string SourcePath { get; set; }

		[DataMember(Name = "addressText", EmitDefaultValue = false)]
		public string AddressText { get; set; }


		[DataMember(Name = "location", EmitDefaultValue = false)]
		public Location Location { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented, jsonSerializerSettings);
		}
	}
}
