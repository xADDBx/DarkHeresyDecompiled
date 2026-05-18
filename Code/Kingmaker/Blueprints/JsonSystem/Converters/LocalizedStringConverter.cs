using System;
using Kingmaker.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kingmaker.Blueprints.JsonSystem.Converters;

public class LocalizedStringConverter : JsonConverter
{
	private const string Prefix = "!str_";

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(LocalizedString);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value is LocalizedString localizedString && !string.IsNullOrEmpty(localizedString.Key) && Guid.TryParse(localizedString.Key, out var result))
		{
			writer.WriteValue("!str_" + result.ToString("N"));
		}
		else
		{
			writer.WriteNull();
		}
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		LocalizedString localizedString = new LocalizedString();
		if (reader.TokenType == JsonToken.Null)
		{
			return localizedString;
		}
		if (reader.TokenType == JsonToken.StartObject && JObject.Load(reader).TryGetValue("m_Key", out JToken value))
		{
			localizedString.Key = value.ToString();
		}
		if (reader.TokenType == JsonToken.String)
		{
			string text = reader.Value as string;
			if (text != null)
			{
				if (text.StartsWith("!str_"))
				{
					text = text.Substring("!str_".Length);
				}
				if (Guid.TryParse((text.Length == 32) ? text : text.Replace("-", ""), out var result))
				{
					localizedString.Key = result.ToString("D");
				}
			}
		}
		return localizedString;
	}
}
