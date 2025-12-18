using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kingmaker.Blueprints.JsonSystem.Converters;

public class StringOrArrayConverter : JsonConverter<List<string>>
{
	public override List<string> ReadJson(JsonReader reader, Type objectType, List<string> existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JToken jToken = JToken.Load(reader);
		if (jToken.Type == JTokenType.String)
		{
			return new List<string>();
		}
		if (jToken.Type == JTokenType.Array)
		{
			return jToken.ToObject<List<string>>();
		}
		throw new JsonSerializationException("Expected string or array for List<string> conversion.");
	}

	public override void WriteJson(JsonWriter writer, List<string> value, JsonSerializer serializer)
	{
		writer.WriteStartArray();
		foreach (string item in value)
		{
			writer.WriteValue(item);
		}
		writer.WriteEndArray();
	}
}
