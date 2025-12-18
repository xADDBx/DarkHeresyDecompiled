using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Owlcat.BehaviourTrees;

namespace Kingmaker.Blueprints.JsonSystem.Converters;

public class BehaviourTreeReferenceConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		string text = null;
		if (value is BehaviourTreeReference behaviourTreeReference)
		{
			text = behaviourTreeReference.AssetGuid;
		}
		else
		{
			writer.WriteNull();
		}
		if (string.IsNullOrEmpty(text))
		{
			writer.WriteNull();
			return;
		}
		writer.WriteStartObject();
		writer.WritePropertyName("AssetGuid");
		writer.WriteValue("!bp_" + text);
		writer.WriteEndObject();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		object obj = Activator.CreateInstance(objectType);
		if (reader.TokenType == JsonToken.Null)
		{
			return obj;
		}
		string text = (string?)JObject.Load(reader)["AssetGuid"];
		if (!string.IsNullOrEmpty(text))
		{
			text = (text.StartsWith("!bp_") ? text.Substring(4) : text);
			if (obj is BehaviourTreeReference behaviourTreeReference)
			{
				behaviourTreeReference.AssetGuid = text;
			}
		}
		return obj;
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(BehaviourTreeReference);
	}
}
