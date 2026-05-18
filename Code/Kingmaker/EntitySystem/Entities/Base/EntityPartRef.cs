using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities.Base;

[JsonObject(IsReference = false)]
[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public struct EntityPartRef<TEntity, TPart> : IEquatable<EntityPartRef<TEntity, TPart>>, IMemoryPackable<EntityPartRef<TEntity, TPart>>, IMemoryPackFormatterRegister, IHashable, IOwlPackable, IOwlPackable<EntityPartRef<TEntity, TPart>> where TEntity : Entity where TPart : EntityPart<TEntity>, new()
{
	[Preserve]
	private sealed class EntityPartRefFormatter : MemoryPackFormatter<EntityPartRef<TEntity, TPart>>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref EntityPartRef<TEntity, TPart> value)
		{
			EntityPartRef<TEntity, TPart>.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref EntityPartRef<TEntity, TPart> value)
		{
			EntityPartRef<TEntity, TPart>.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref EntityPartRef<TEntity, TPart> value)
		{
			EntityPartRef<TEntity, TPart>.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref EntityPartRef<TEntity, TPart> value)
		{
			EntityPartRef<TEntity, TPart>.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly EntityRef<TEntity> m_Ref;

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
			return entity.GetOptional<TPart>();
		}
	}

	[MemoryPackConstructor]
	private EntityPartRef(EntityRef<TEntity> m_ref)
	{
		m_Ref = m_ref;
	}

	public EntityPartRef([CanBeNull] string id)
		: this(new EntityRef<TEntity>(id))
	{
	}

	public EntityPartRef([CanBeNull] TEntity entity)
		: this(new EntityRef<TEntity>(entity))
	{
	}

	public override int GetHashCode()
	{
		return m_Ref.GetHashCode();
	}

	public static implicit operator EntityPartRef<TEntity, TPart>([CanBeNull] TPart part)
	{
		return new EntityPartRef<TEntity, TPart>((part != null) ? part.Owner : null);
	}

	public static implicit operator EntityPartRef<TEntity, TPart>([CanBeNull] TEntity entity)
	{
		return new EntityPartRef<TEntity, TPart>(entity);
	}

	public static implicit operator TEntity(EntityPartRef<TEntity, TPart> @ref)
	{
		return @ref.Entity;
	}

	public static implicit operator TPart(EntityPartRef<TEntity, TPart> @ref)
	{
		return @ref.EntityPart;
	}

	public bool Equals(EntityPartRef<TEntity, TPart> other)
	{
		return m_Ref.Equals(other.m_Ref);
	}

	public override bool Equals(object obj)
	{
		if (obj is EntityPartRef<TEntity, TPart> other)
		{
			return Equals(other);
		}
		return false;
	}

	static EntityPartRef()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "EntityPartRef",
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_Ref", typeof(EntityRef<TEntity>))
			}
		};
		m_Serializer_EntityRef_ = null;
		m_Deserializer_EntityRef_ = null;
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<EntityPartRef<TEntity, TPart>>())
		{
			MemoryPackFormatterProvider.Register(new EntityPartRefFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EntityPartRef<TEntity, TPart>[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<EntityPartRef<TEntity, TPart>>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref EntityPartRef<TEntity, TPart> value) where TBufferWriter : class, IBufferWriter<byte>
	{
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_Ref);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref EntityPartRef<TEntity, TPart> value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(EntityPartRef<TEntity, TPart>);
			return;
		}
		EntityRef<TEntity> value2;
		if (memberCount == 1)
		{
			value2 = reader.ReadPackable<EntityRef<TEntity>>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(EntityPartRef<TEntity, TPart>), 1, memberCount);
				return;
			}
			value2 = default(EntityRef<TEntity>);
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
		}
		value = new EntityPartRef<TEntity, TPart>(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref EntityPartRef<TEntity, TPart> value)
	{
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Ref");
		writer.WritePackable(value.m_Ref);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref EntityPartRef<TEntity, TPart> value)
	{
		if (!reader.CheckObjectStart())
		{
			value = default(EntityPartRef<TEntity, TPart>);
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityRef<TEntity> @ref = default(EntityRef<TEntity>);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (text == "m_Ref")
			{
				@ref = reader.ReadPackable<EntityRef<TEntity>>();
				array[0] = true;
			}
		}
		value = new EntityPartRef<TEntity, TPart>(@ref);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<TEntity> obj = m_Ref;
		Hash128 val = StructHasher<EntityRef<TEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		EntityPartRef<TEntity, TPart> source = default(EntityPartRef<TEntity, TPart>);
		result = Unsafe.As<EntityPartRef<TEntity, TPart>, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<EntityPartRef<TEntity, TPart>>(OwlPackTypeInfo);
		OutputFormatter.CreateFieldDelegate(formatter, ref m_Serializer_EntityRef_);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityRef<TEntity> value = m_Ref;
		formatter.Field(0, "m_Ref", ref value, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		InputFormatter.CreateFieldDelegate(formatter, ref m_Deserializer_EntityRef_);
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EntityPartRef<TEntity, TPart>>();
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
			}
		}
		formatter.LeaveObject();
	}
}
