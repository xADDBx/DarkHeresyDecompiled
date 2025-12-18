using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem;

[JsonObject(IsReference = false)]
[OwlPackable(OwlPackableMode.Generate)]
public struct EntityFactRef : IEquatable<EntityFactRef>, IHashable, IOwlPackable, IOwlPackable<EntityFactRef>
{
	private EntityServiceProxy m_Proxy;

	[JsonProperty]
	[OwlPackInclude]
	public readonly string EntityId;

	[JsonProperty]
	[OwlPackInclude]
	public readonly string FactId;

	private WeakReference<EntityFact> m_FactCache;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "EntityFactRef",
		Fields = new FieldInfo[2]
		{
			new FieldInfo("EntityId", typeof(string)),
			new FieldInfo("FactId", typeof(string))
		}
	};

	public bool IsEmpty => string.IsNullOrEmpty(EntityId);

	[CanBeNull]
	public Entity Entity
	{
		get
		{
			if (!IsEmpty && (m_Proxy == null || m_Proxy.IsDisposed))
			{
				m_Proxy = EntityService.Instance?.GetProxy(EntityId);
			}
			return (Entity)(m_Proxy?.Entity);
		}
	}

	[CanBeNull]
	public EntityFact Fact
	{
		get
		{
			if (IsEmpty)
			{
				return null;
			}
			if (m_FactCache == null)
			{
				m_FactCache = new WeakReference<EntityFact>(null);
			}
			if (m_FactCache.TryGetTarget(out var target))
			{
				return target;
			}
			EntityFact entityFact = Entity?.Facts.FindById(FactId);
			m_FactCache.SetTarget(entityFact);
			return entityFact;
		}
	}

	public EntityFactRef([CanBeNull] string entityId, [CanBeNull] string factId)
	{
		EntityId = entityId;
		FactId = factId;
		m_Proxy = null;
		m_FactCache = new WeakReference<EntityFact>(null);
	}

	public EntityFactRef([CanBeNull] EntityFact fact)
	{
		if (fact?.Owner != null)
		{
			EntityId = fact.Owner.UniqueId;
			FactId = fact.UniqueId;
			m_Proxy = fact.Owner.Proxy;
			m_FactCache = new WeakReference<EntityFact>(null);
		}
		else
		{
			EntityId = null;
			FactId = null;
			m_Proxy = null;
			m_FactCache = new WeakReference<EntityFact>(null);
		}
	}

	public readonly bool Equals(EntityFactRef other)
	{
		if (string.Equals(EntityId, other.EntityId))
		{
			return string.Equals(FactId, other.FactId);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is EntityFactRef other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (EntityId == null)
		{
			return 0;
		}
		return EntityId.GetHashCode();
	}

	public static implicit operator EntityFactRef([CanBeNull] EntityFact fact)
	{
		return new EntityFactRef(fact);
	}

	public static bool operator ==(EntityFactRef r, [CanBeNull] EntityFact fact)
	{
		if (r.EntityId == fact?.Owner?.UniqueId)
		{
			return r.FactId == fact?.UniqueId;
		}
		return false;
	}

	public static bool operator !=(EntityFactRef r, [CanBeNull] EntityFact fact)
	{
		if (!(r.EntityId != fact?.Owner?.UniqueId))
		{
			return r.FactId != fact?.UniqueId;
		}
		return true;
	}

	public static implicit operator EntityFact(EntityFactRef @ref)
	{
		return @ref.Fact;
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(EntityId);
		result.Append(FactId);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		EntityFactRef source = default(EntityFactRef);
		result = Unsafe.As<EntityFactRef, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<EntityFactRef>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = EntityId;
		formatter.StringField(0, "EntityId", ref value, state);
		string value2 = FactId;
		formatter.StringField(1, "FactId", ref value2, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EntityFactRef>();
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
				Unsafe.AsRef(in EntityId) = formatter.ReadString(state);
				break;
			case 1:
				Unsafe.AsRef(in FactId) = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
[JsonObject(IsReference = false)]
[OwlPackable(OwlPackableMode.Generate)]
public struct EntityFactRef<T> : IEquatable<EntityFactRef<T>>, IHashable, IOwlPackable, IOwlPackable<EntityFactRef<T>> where T : EntityFact
{
	private EntityServiceProxy m_Proxy;

	[JsonProperty]
	[OwlPackInclude]
	public readonly string EntityId;

	[JsonProperty]
	[OwlPackInclude]
	public readonly string FactId;

	private WeakReference<T> m_FactCache;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "EntityFactRef",
		Fields = new FieldInfo[2]
		{
			new FieldInfo("EntityId", typeof(string)),
			new FieldInfo("FactId", typeof(string))
		}
	};

	public bool IsEmpty
	{
		get
		{
			if (!string.IsNullOrEmpty(EntityId))
			{
				return string.IsNullOrEmpty(FactId);
			}
			return true;
		}
	}

	[CanBeNull]
	public Entity Entity
	{
		get
		{
			if (!IsEmpty && (m_Proxy == null || m_Proxy.IsDisposed))
			{
				m_Proxy = EntityService.Instance?.GetProxy(EntityId);
			}
			return (Entity)(m_Proxy?.Entity);
		}
	}

	[CanBeNull]
	public T Fact
	{
		get
		{
			if (IsEmpty)
			{
				return null;
			}
			if (m_FactCache == null)
			{
				m_FactCache = new WeakReference<T>(null);
			}
			if (m_FactCache.TryGetTarget(out var target))
			{
				return target;
			}
			T val = Entity?.Facts.FindById(FactId) as T;
			m_FactCache.SetTarget(val);
			return val;
		}
	}

	public EntityFactRef([CanBeNull] string entityId, [CanBeNull] string factId)
	{
		EntityId = entityId;
		FactId = factId;
		m_Proxy = null;
		m_FactCache = new WeakReference<T>(null);
	}

	public EntityFactRef([CanBeNull] T fact)
	{
		if (fact?.Owner != null)
		{
			EntityId = fact.Owner.UniqueId;
			FactId = fact.UniqueId;
			m_Proxy = fact.Owner.Proxy;
			m_FactCache = new WeakReference<T>(null);
		}
		else
		{
			EntityId = null;
			FactId = null;
			m_Proxy = null;
			m_FactCache = new WeakReference<T>(null);
		}
	}

	public bool Equals(EntityFactRef<T> other)
	{
		if (string.Equals(EntityId, other.EntityId))
		{
			return string.Equals(FactId, other.FactId);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is EntityFactRef<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (EntityId == null)
		{
			return 0;
		}
		return EntityId.GetHashCode();
	}

	public static implicit operator EntityFactRef<T>([CanBeNull] T fact)
	{
		return new EntityFactRef<T>(fact);
	}

	public static bool operator ==(EntityFactRef<T> r, [CanBeNull] T fact)
	{
		if (r.EntityId == fact?.Owner?.UniqueId)
		{
			return r.FactId == fact?.UniqueId;
		}
		return false;
	}

	public static bool operator !=(EntityFactRef<T> r, [CanBeNull] T fact)
	{
		if (!(r.EntityId != fact?.Owner?.UniqueId))
		{
			return r.FactId != fact?.UniqueId;
		}
		return true;
	}

	public static implicit operator T(EntityFactRef<T> @ref)
	{
		return @ref.Fact;
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(EntityId);
		result.Append(FactId);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		EntityFactRef<T> source = default(EntityFactRef<T>);
		result = Unsafe.As<EntityFactRef<T>, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<EntityFactRef<T>>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = EntityId;
		formatter.StringField(0, "EntityId", ref value, state);
		string value2 = FactId;
		formatter.StringField(1, "FactId", ref value2, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EntityFactRef<T>>();
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
				Unsafe.AsRef(in EntityId) = formatter.ReadString(state);
				break;
			case 1:
				Unsafe.AsRef(in FactId) = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
