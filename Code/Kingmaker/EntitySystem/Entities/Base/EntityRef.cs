using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities.Base;

[HashRoot]
[JsonObject(IsReference = false)]
[OwlPackable(OwlPackableMode.Generate)]
public struct EntityRef : IEquatable<EntityRef>, IEntityRef, IOwlPackable, IOwlPackable<EntityRef>
{
	private EntityServiceProxy m_Proxy;

	[OwlPackInclude]
	public readonly string Id;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "EntityRef",
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Id", typeof(string))
		}
	};

	string IEntityRef.Id => Id;

	public bool IsEmpty
	{
		get
		{
			if (string.IsNullOrEmpty(Id))
			{
				return m_Proxy == null;
			}
			return false;
		}
	}

	[CanBeNull]
	public IEntity Entity
	{
		get
		{
			if ((m_Proxy == null || m_Proxy.IsDisposed) && !IsEmpty)
			{
				m_Proxy = EntityService.Instance?.GetProxy(Id);
			}
			return m_Proxy?.Entity;
		}
	}

	public EntityRef([CanBeNull] string id)
	{
		Id = id;
		m_Proxy = (string.IsNullOrEmpty(id) ? null : EntityService.Instance?.GetProxy(Id));
	}

	public EntityRef([CanBeNull] IEntity entity)
	{
		Id = entity?.UniqueId;
		m_Proxy = entity?.Proxy;
	}

	[CanBeNull]
	public T Get<T>() where T : class, IEntity
	{
		return Entity as T;
	}

	[CanBeNull]
	public IEntity Get()
	{
		return Entity;
	}

	public readonly bool Equals(EntityRef other)
	{
		return string.Equals(Id, other.Id, StringComparison.Ordinal);
	}

	public override bool Equals(object obj)
	{
		if (obj is EntityRef other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (Id == null)
		{
			return 0;
		}
		return Id.GetHashCode();
	}

	public static bool operator ==(EntityRef r, [CanBeNull] IEntity entity)
	{
		return string.Equals(r.Id, entity?.UniqueId, StringComparison.Ordinal);
	}

	public static bool operator !=(EntityRef r, [CanBeNull] IEntity entity)
	{
		return !string.Equals(r.Id, entity?.UniqueId, StringComparison.Ordinal);
	}

	public static bool operator ==(EntityRef r1, EntityRef r2)
	{
		return string.Equals(r1.Id, r2.Id, StringComparison.Ordinal);
	}

	public static bool operator !=(EntityRef r1, EntityRef r2)
	{
		return !string.Equals(r1.Id, r2.Id, StringComparison.Ordinal);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		EntityRef source = default(EntityRef);
		result = Unsafe.As<EntityRef, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<EntityRef>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = Id;
		formatter.StringField(0, "Id", ref value, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EntityRef>();
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
				Unsafe.AsRef(in Id) = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
[HashRoot]
[JsonObject(IsReference = false)]
[OwlPackable(OwlPackableMode.Generate)]
public struct EntityRef<T> : IEntityRef, ITypedEntityRef, IEquatable<EntityRef<T>>, IComparable<EntityRef<T>>, IHashable, IOwlPackable, IOwlPackable<EntityRef<T>> where T : class, IEntity
{
	public sealed class Comparer : IComparer<EntityRef<T>>
	{
		public static Comparer Instance = new Comparer();

		int IComparer<EntityRef<T>>.Compare(EntityRef<T> a, EntityRef<T> b)
		{
			return a.CompareTo(b);
		}
	}

	private EntityServiceProxy m_Proxy;

	[JsonProperty]
	[OwlPackInclude]
	public readonly string Id;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "EntityRef",
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Id", typeof(string))
		}
	};

	string IEntityRef.Id => Id;

	public bool IsNull => string.IsNullOrEmpty(Id);

	[CanBeNull]
	public T Entity
	{
		get
		{
			if (!IsNull && (m_Proxy == null || m_Proxy.IsDisposed))
			{
				m_Proxy = EntityService.Instance?.GetProxy(Id);
			}
			return m_Proxy?.Entity as T;
		}
	}

	public EntityRef([CanBeNull] string id)
	{
		Id = id;
		m_Proxy = (string.IsNullOrEmpty(id) ? null : EntityService.Instance?.GetProxy(id));
	}

	public EntityRef([CanBeNull] T entity)
	{
		Id = entity?.UniqueId;
		m_Proxy = entity?.Proxy;
	}

	[CanBeNull]
	public TEntity Get<TEntity>() where TEntity : class, IEntity
	{
		return Entity as TEntity;
	}

	[CanBeNull]
	public IEntity Get()
	{
		return Entity;
	}

	public bool Equals(EntityRef<T> other)
	{
		return string.Equals(Id, other.Id, StringComparison.Ordinal);
	}

	public override bool Equals(object obj)
	{
		if (obj is EntityRef<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Id?.GetHashCode() ?? 0;
	}

	public string GetId()
	{
		return Id;
	}

	public static implicit operator EntityRef(EntityRef<T> @ref)
	{
		return new EntityRef(@ref.Id);
	}

	public static implicit operator EntityRef<T>([CanBeNull] T entity)
	{
		return new EntityRef<T>(entity);
	}

	public static bool operator ==(EntityRef<T> r, [CanBeNull] T entity)
	{
		return string.Equals(r.Id, entity?.UniqueId, StringComparison.Ordinal);
	}

	public static bool operator !=(EntityRef<T> r, [CanBeNull] T entity)
	{
		return !string.Equals(r.Id, entity?.UniqueId, StringComparison.Ordinal);
	}

	public static implicit operator T(EntityRef<T> @ref)
	{
		return @ref.Entity;
	}

	public int CompareTo(EntityRef<T> other)
	{
		return string.Compare(Id, other.Id, StringComparison.Ordinal);
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(Id);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		EntityRef<T> source = default(EntityRef<T>);
		result = Unsafe.As<EntityRef<T>, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<EntityRef<T>>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = Id;
		formatter.StringField(0, "Id", ref value, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EntityRef<T>>();
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
				Unsafe.AsRef(in Id) = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
