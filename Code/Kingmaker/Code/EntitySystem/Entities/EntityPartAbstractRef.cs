using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Code.EntitySystem.Entities;

[JsonObject(IsReference = false)]
[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
[OwlPackOldName("Kingmaker.Code.EntitySystem.Entities.ViewBasedPartRef`2")]
public struct EntityPartAbstractRef<TEntity, TPart> : IEquatable<EntityPartAbstractRef<TEntity, TPart>>, IMemoryPackable<EntityPartAbstractRef<TEntity, TPart>>, IMemoryPackFormatterRegister, IOwlPackable, IOwlPackable<EntityPartAbstractRef<TEntity, TPart>> where TEntity : Entity where TPart : EntityPart
{
	[Preserve]
	private sealed class EntityPartAbstractRefFormatter : MemoryPackFormatter<EntityPartAbstractRef<TEntity, TPart>>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref EntityPartAbstractRef<TEntity, TPart> value)
		{
			EntityPartAbstractRef<TEntity, TPart>.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref EntityPartAbstractRef<TEntity, TPart> value)
		{
			EntityPartAbstractRef<TEntity, TPart>.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref EntityPartAbstractRef<TEntity, TPart> value)
		{
			EntityPartAbstractRef<TEntity, TPart>.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref EntityPartAbstractRef<TEntity, TPart> value)
		{
			EntityPartAbstractRef<TEntity, TPart>.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly EntityRef<TEntity> m_Ref;

	[NotNull]
	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly Type m_PartType;

	public static readonly TypeInfo OwlPackTypeInfo;

	private static IOutputFormatter.FieldDelegate<EntityRef<TEntity>> m_Serializer_EntityRef_;

	private static IInputFormatter.FieldDelegate<EntityRef<TEntity>> m_Deserializer_EntityRef_;

	[MemoryPackIgnore]
	public bool IsNull => m_Ref.IsNull;

	[MemoryPackIgnore]
	public EntityRef<TEntity> EntityRef => m_Ref;

	[CanBeNull]
	[MemoryPackIgnore]
	public TEntity Entity => m_Ref.Entity;

	[CanBeNull]
	[MemoryPackIgnore]
	public TPart EntityPart
	{
		get
		{
			TEntity entity = Entity;
			if (entity == null)
			{
				return null;
			}
			return entity.GetOptional<TPart>(m_PartType);
		}
	}

	[MemoryPackConstructor]
	private EntityPartAbstractRef(EntityRef<TEntity> m_ref, Type m_partType)
	{
		m_Ref = m_ref;
		m_PartType = m_partType;
	}

	public EntityPartAbstractRef([NotNull] TPart part)
	{
		m_Ref = new EntityRef<TEntity>((TEntity)part.Owner);
		m_PartType = part.GetType();
		CheckType(m_PartType);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(m_Ref, m_PartType);
	}

	public static implicit operator TEntity(EntityPartAbstractRef<TEntity, TPart> @ref)
	{
		return @ref.Entity;
	}

	public static implicit operator TPart(EntityPartAbstractRef<TEntity, TPart> @ref)
	{
		return @ref.EntityPart;
	}

	public bool Equals(EntityPartAbstractRef<TEntity, TPart> other)
	{
		if (m_Ref.Equals(other.m_Ref))
		{
			return m_PartType == other.m_PartType;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is EntityPartAbstractRef<TEntity, TPart> other)
		{
			return Equals(other);
		}
		return false;
	}

	private static void CheckType(Type type)
	{
	}

	static EntityPartAbstractRef()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "EntityPartAbstractRef",
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_Ref", typeof(EntityRef<TEntity>)),
				new FieldInfo("m_PartType", typeof(Type))
			}
		};
		m_Serializer_EntityRef_ = null;
		m_Deserializer_EntityRef_ = null;
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<EntityPartAbstractRef<TEntity, TPart>>())
		{
			MemoryPackFormatterProvider.Register(new EntityPartAbstractRefFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EntityPartAbstractRef<TEntity, TPart>[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<EntityPartAbstractRef<TEntity, TPart>>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref EntityPartAbstractRef<TEntity, TPart> value) where TBufferWriter : class, IBufferWriter<byte>
	{
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_Ref);
		writer.WriteValue(in value.m_PartType);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref EntityPartAbstractRef<TEntity, TPart> value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(EntityPartAbstractRef<TEntity, TPart>);
			return;
		}
		EntityRef<TEntity> value2;
		Type value3;
		if (memberCount == 2)
		{
			value2 = reader.ReadPackable<EntityRef<TEntity>>();
			value3 = reader.ReadValue<Type>();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(EntityPartAbstractRef<TEntity, TPart>), 2, memberCount);
				return;
			}
			value2 = default(EntityRef<TEntity>);
			value3 = null;
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadValue(ref value3);
					_ = 2;
				}
			}
		}
		value = new EntityPartAbstractRef<TEntity, TPart>(value2, value3);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref EntityPartAbstractRef<TEntity, TPart> value)
	{
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Ref");
		writer.WritePackable(value.m_Ref);
		writer.WriteProperty("m_PartType");
		writer.WriteValue(value.m_PartType);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref EntityPartAbstractRef<TEntity, TPart> value)
	{
		if (!reader.CheckObjectStart())
		{
			value = default(EntityPartAbstractRef<TEntity, TPart>);
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityRef<TEntity> @ref = default(EntityRef<TEntity>);
		Type partType = null;
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (!(text == "m_Ref"))
			{
				if (text == "m_PartType")
				{
					partType = reader.ReadValue<Type>();
					array[1] = true;
				}
			}
			else
			{
				@ref = reader.ReadPackable<EntityRef<TEntity>>();
				array[0] = true;
			}
		}
		value = new EntityPartAbstractRef<TEntity, TPart>(@ref, partType);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		EntityPartAbstractRef<TEntity, TPart> source = default(EntityPartAbstractRef<TEntity, TPart>);
		result = Unsafe.As<EntityPartAbstractRef<TEntity, TPart>, TPossiblyBase>(ref source);
	}

	public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<EntityPartAbstractRef<TEntity, TPart>>(OwlPackTypeInfo);
		OutputFormatter.CreateFieldDelegate(formatter, ref m_Serializer_EntityRef_);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityRef<TEntity> value = m_Ref;
		formatter.Field(0, "m_Ref", ref value, state);
		Type value2 = m_PartType;
		formatter.Field(1, "m_PartType", ref value2, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		InputFormatter.CreateFieldDelegate(formatter, ref m_Deserializer_EntityRef_);
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EntityPartAbstractRef<TEntity, TPart>>();
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
				Unsafe.AsRef(in m_Ref) = formatter.ReadPackable<EntityRef<TEntity>>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_PartType) = formatter.ReadPackable<Type>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
