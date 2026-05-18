using System;
using Newtonsoft.Json;

namespace Kingmaker.Blueprints.JsonSystem.Converters;

public class DateTimeConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		writer.WriteValue(((DateTimeOffset)value).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"));
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		object value = reader.Value;
		DateTimeOffset dateTimeOffset = ((value is DateTime dateTime) ? new DateTimeOffset(dateTime) : ((!(value is string input)) ? DateTimeOffset.UtcNow : DateTimeOffset.Parse(input)));
		return dateTimeOffset;
	}

	public override bool CanConvert(Type objectType)
	{
		if (!(objectType == typeof(DateTimeOffset)))
		{
			return objectType == typeof(DateTimeOffset?);
		}
		return true;
	}
}
