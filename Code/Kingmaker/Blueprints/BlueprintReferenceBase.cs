using System;
using System.Buffers;
using Kingmaker.Blueprints.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
[MemoryPackable(GenerateType.Object)]
public class BlueprintReferenceBase : IEquatable<BlueprintReferenceBase>, IReferenceBase, IMemoryPackable<BlueprintReferenceBase>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintReferenceBaseFormatter : MemoryPackFormatter<BlueprintReferenceBase>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref BlueprintReferenceBase value)
		{
			BlueprintReferenceBase.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref BlueprintReferenceBase value)
		{
			BlueprintReferenceBase.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintReferenceBase value)
		{
			BlueprintReferenceBase.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref BlueprintReferenceBase value)
		{
			BlueprintReferenceBase.DeserializeJson(ref reader, ref value);
		}
	}

	[SerializeField]
	[HideInInspector]
	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	protected string guid;

	private BlueprintScriptableObject Cached { get; set; }

	[MemoryPackIgnore]
	[JsonIgnore]
	public string Guid => guid;

	[MemoryPackIgnore]
	[JsonIgnore]
	public SimpleBlueprint Blueprint => GetBlueprint();

	[MemoryPackConstructor]
	protected BlueprintReferenceBase()
	{
	}

	public BlueprintScriptableObject GetBlueprint()
	{
		if (Cached == null)
		{
			Cached = ResourcesLibrary.TryGetBlueprint(guid) as BlueprintScriptableObject;
		}
		return Cached;
	}

	public bool IsNull()
	{
		if (!string.IsNullOrEmpty(guid))
		{
			return GetBlueprint() == null;
		}
		return true;
	}

	public bool IsEmpty()
	{
		if (!string.IsNullOrEmpty(guid))
		{
			return !GetBlueprint();
		}
		return true;
	}

	public static TRef CreateTyped<TRef>(BlueprintScriptableObject bp) where TRef : BlueprintReferenceBase, new()
	{
		return new TRef
		{
			guid = bp?.AssetGuid
		};
	}

	public bool Equals(BlueprintReferenceBase other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return guid == other.guid;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() == GetType())
		{
			return Equals((BlueprintReferenceBase)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (guid == null)
		{
			return 0;
		}
		return guid.GetHashCode();
	}

	public void ReadGuidFromJson(string value)
	{
		guid = value;
	}

	public static TRef CreateCopy<TRef>(TRef source) where TRef : BlueprintReferenceBase, new()
	{
		return new TRef
		{
			guid = source.guid
		};
	}

	static BlueprintReferenceBase()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintReferenceBase>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintReferenceBaseFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintReferenceBase[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintReferenceBase>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref BlueprintReferenceBase? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WriteString(value.guid);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintReferenceBase? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string text;
		if (memberCount == 1)
		{
			if (value != null)
			{
				text = value.guid;
				text = reader.ReadString();
				goto IL_0068;
			}
			text = reader.ReadString();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintReferenceBase), 1, memberCount);
				return;
			}
			text = ((value != null) ? value.guid : null);
			if (memberCount != 0)
			{
				text = reader.ReadString();
				_ = 1;
			}
			if (value != null)
			{
				goto IL_0068;
			}
		}
		value = new BlueprintReferenceBase
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref BlueprintReferenceBase? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("guid");
		writer.WriteString(value.guid);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref BlueprintReferenceBase? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		string text = ((value != null) ? value.guid : null);
		bool[] array = new bool[1];
		string text2 = null;
		while ((text2 = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text2 == "guid")
				{
					text = reader.ReadString();
					array[0] = true;
				}
			}
			else if (text2 == "guid")
			{
				text = reader.ReadString();
			}
		}
		if (value != null)
		{
			value.guid = text;
		}
		else
		{
			value = new BlueprintReferenceBase
			{
				guid = text
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}
}
