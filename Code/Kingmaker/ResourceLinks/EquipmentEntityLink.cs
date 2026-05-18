using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Visual.CharacterSystem;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.ResourceLinks;

[Serializable]
[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class EquipmentEntityLink : WeakResourceLink<EquipmentEntity>, IMemoryPackable<EquipmentEntityLink>, IMemoryPackFormatterRegister, IHashable, IOwlPackable, IOwlPackable<EquipmentEntityLink>
{
	[Preserve]
	private sealed class EquipmentEntityLinkFormatter : MemoryPackFormatter<EquipmentEntityLink>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref EquipmentEntityLink value)
		{
			EquipmentEntityLink.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref EquipmentEntityLink value)
		{
			EquipmentEntityLink.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref EquipmentEntityLink value)
		{
			EquipmentEntityLink.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref EquipmentEntityLink value)
		{
			EquipmentEntityLink.DeserializeJson(ref reader, ref value);
		}
	}

	public static readonly TypeInfo OwlPackTypeInfo;

	static EquipmentEntityLink()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "EquipmentEntityLink",
			OldNames = null,
			Fields = new FieldInfo[0]
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<EquipmentEntityLink>())
		{
			MemoryPackFormatterProvider.Register(new EquipmentEntityLinkFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EquipmentEntityLink[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<EquipmentEntityLink>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref EquipmentEntityLink? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WriteString(value.AssetId);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref EquipmentEntityLink? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string assetId;
		if (memberCount == 1)
		{
			if (!(value == null))
			{
				assetId = value.AssetId;
				assetId = reader.ReadString();
				goto IL_007a;
			}
			assetId = reader.ReadString();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(EquipmentEntityLink), 1, memberCount);
				return;
			}
			assetId = ((!(value == null)) ? value.AssetId : null);
			if (memberCount != 0)
			{
				assetId = reader.ReadString();
				_ = 1;
			}
			if (!(value == null))
			{
				goto IL_007a;
			}
		}
		value = new EquipmentEntityLink
		{
			AssetId = assetId
		};
		return;
		IL_007a:
		value.AssetId = assetId;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref EquipmentEntityLink? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("AssetId");
		writer.WriteString(value.AssetId);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref EquipmentEntityLink? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		string assetId = ((!(value == null)) ? value.AssetId : null);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "AssetId")
				{
					assetId = reader.ReadString();
					array[0] = true;
				}
			}
			else if (text == "AssetId")
			{
				assetId = reader.ReadString();
			}
		}
		if (!(value == null))
		{
			value.AssetId = assetId;
		}
		else
		{
			value = new EquipmentEntityLink
			{
				AssetId = assetId
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		EquipmentEntityLink source = new EquipmentEntityLink();
		result = Unsafe.As<EquipmentEntityLink, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<EquipmentEntityLink>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EquipmentEntityLink>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
