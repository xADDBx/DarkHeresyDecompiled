using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.MapObjects;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class ItemsCollectionRef : IMemoryPackable<ItemsCollectionRef>, IMemoryPackFormatterRegister, IOwlPackable, IOwlPackable<ItemsCollectionRef>
{
	[Preserve]
	private sealed class ItemsCollectionRefFormatter : MemoryPackFormatter<ItemsCollectionRef>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref ItemsCollectionRef value)
		{
			ItemsCollectionRef.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref ItemsCollectionRef value)
		{
			ItemsCollectionRef.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ItemsCollectionRef value)
		{
			ItemsCollectionRef.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref ItemsCollectionRef value)
		{
			ItemsCollectionRef.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private EntityRef m_OwnerRef;

	public static readonly TypeInfo OwlPackTypeInfo;

	[MemoryPackIgnore]
	public ItemsCollection ItemsCollection
	{
		get
		{
			if (m_OwnerRef.Entity == null)
			{
				return null;
			}
			IEntity entity = m_OwnerRef.Entity;
			if (!(entity is Player))
			{
				if (entity is PartInventory.IOwner owner)
				{
					return owner.Inventory.Collection;
				}
				return m_OwnerRef.Entity.ToEntity().GetOptional<InteractionLootPart>()?.Items;
			}
			return Game.Instance.PartySharedInventory.Collection;
		}
	}

	[MemoryPackConstructor]
	private ItemsCollectionRef()
	{
	}

	[JsonConstructor]
	public ItemsCollectionRef([CanBeNull] ItemsCollection collection)
	{
		m_OwnerRef = new EntityRef(collection?.Owner);
	}

	public bool Equals(ItemsCollectionRef other)
	{
		if (other != null)
		{
			return m_OwnerRef.Equals(other.m_OwnerRef);
		}
		return false;
	}

	static ItemsCollectionRef()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "ItemsCollectionRef",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_OwnerRef", typeof(EntityRef))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ItemsCollectionRef>())
		{
			MemoryPackFormatterProvider.Register(new ItemsCollectionRefFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ItemsCollectionRef[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ItemsCollectionRef>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref ItemsCollectionRef? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_OwnerRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ItemsCollectionRef? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_OwnerRef;
				reader.ReadPackable(ref value2);
				goto IL_0070;
			}
			value2 = reader.ReadPackable<EntityRef>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ItemsCollectionRef), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_OwnerRef : default(EntityRef));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_0070;
			}
		}
		value = new ItemsCollectionRef
		{
			m_OwnerRef = value2
		};
		return;
		IL_0070:
		value.m_OwnerRef = value2;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref ItemsCollectionRef? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_OwnerRef");
		writer.WritePackable(value.m_OwnerRef);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref ItemsCollectionRef? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityRef val = ((value != null) ? value.m_OwnerRef : default(EntityRef));
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_OwnerRef")
				{
					val = reader.ReadPackable<EntityRef>();
					array[0] = true;
				}
			}
			else if (text == "m_OwnerRef")
			{
				reader.ReadPackable(ref val);
			}
		}
		if (value != null)
		{
			value.m_OwnerRef = val;
		}
		else
		{
			value = new ItemsCollectionRef
			{
				m_OwnerRef = val
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ItemsCollectionRef source = new ItemsCollectionRef();
		result = Unsafe.As<ItemsCollectionRef, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ItemsCollectionRef>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_OwnerRef", ref m_OwnerRef, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ItemsCollectionRef>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				m_OwnerRef = formatter.ReadPackable<EntityRef>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
