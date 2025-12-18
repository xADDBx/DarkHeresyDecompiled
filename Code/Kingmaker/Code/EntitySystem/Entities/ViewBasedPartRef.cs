using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Code.EntitySystem.Entities;

[JsonObject(IsReference = false)]
[OwlPackable(OwlPackableMode.Generate)]
public struct ViewBasedPartRef<TEntity, TPart> : IEquatable<ViewBasedPartRef<TEntity, TPart>>, IOwlPackable, IOwlPackable<ViewBasedPartRef<TEntity, TPart>> where TEntity : Entity where TPart : ViewBasedPart
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly EntityRef<TEntity> m_Ref;

	[NotNull]
	[JsonProperty]
	[OwlPackInclude]
	private readonly Type m_PartType;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ViewBasedPartRef",
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_Ref", typeof(EntityRef<TEntity>)),
			new FieldInfo("m_PartType", typeof(Type))
		}
	};

	private static IOutputFormatter.FieldDelegate<EntityRef<TEntity>> m_Serializer_EntityRef_ = null;

	private static IInputFormatter.FieldDelegate<EntityRef<TEntity>> m_Deserializer_EntityRef_ = null;

	public bool IsNull => m_Ref.IsNull;

	public EntityRef<TEntity> EntityRef => m_Ref;

	[CanBeNull]
	public TEntity Entity => m_Ref.Entity;

	[CanBeNull]
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

	private ViewBasedPartRef(EntityRef<TEntity> m_ref, [NotNull] Type m_partType)
	{
		m_Ref = m_ref;
		m_PartType = m_partType;
	}

	public ViewBasedPartRef([CanBeNull] string id, [CanBeNull] Type type)
	{
		m_Ref = new EntityRef<TEntity>(id);
		m_PartType = type ?? typeof(TPart);
		CheckType(m_PartType);
	}

	public ViewBasedPartRef([CanBeNull] TEntity entity, [CanBeNull] Type type)
	{
		m_Ref = new EntityRef<TEntity>(entity);
		m_PartType = type ?? typeof(TPart);
		CheckType(m_PartType);
	}

	public ViewBasedPartRef([CanBeNull] TPart part, [CanBeNull] Type type)
	{
		m_Ref = new EntityRef<TEntity>((TEntity)(part?.Owner));
		m_PartType = type ?? typeof(TPart);
		CheckType(m_PartType);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(m_Ref, m_PartType);
	}

	public static implicit operator ViewBasedPartRef<TEntity, TPart>([CanBeNull] TPart part)
	{
		return new ViewBasedPartRef<TEntity, TPart>(part, null);
	}

	public static implicit operator ViewBasedPartRef<TEntity, TPart>([CanBeNull] TEntity entity)
	{
		return new ViewBasedPartRef<TEntity, TPart>(entity, null);
	}

	public static implicit operator TEntity(ViewBasedPartRef<TEntity, TPart> @ref)
	{
		return @ref.Entity;
	}

	public static implicit operator TPart(ViewBasedPartRef<TEntity, TPart> @ref)
	{
		return @ref.EntityPart;
	}

	public bool Equals(ViewBasedPartRef<TEntity, TPart> other)
	{
		if (m_Ref.Equals(other.m_Ref))
		{
			return m_PartType == other.m_PartType;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is ViewBasedPartRef<TEntity, TPart> other)
		{
			return Equals(other);
		}
		return false;
	}

	private static void CheckType(Type type)
	{
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ViewBasedPartRef<TEntity, TPart> source = default(ViewBasedPartRef<TEntity, TPart>);
		result = Unsafe.As<ViewBasedPartRef<TEntity, TPart>, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ViewBasedPartRef<TEntity, TPart>>(OwlPackTypeInfo);
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
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ViewBasedPartRef<TEntity, TPart>>();
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
