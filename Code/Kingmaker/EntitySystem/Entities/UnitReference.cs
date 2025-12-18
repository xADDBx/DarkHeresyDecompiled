using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Code.Framework.Utility.UnityExtensions;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;

namespace Kingmaker.EntitySystem.Entities;

[JsonObject(IsReference = false)]
[OwlPackable(OwlPackableMode.Generate)]
[HashRoot]
public struct UnitReference : IEntityRef, IEquatable<UnitReference>, IComparable<UnitReference>, IOwlPackable, IOwlPackable<UnitReference>
{
	public static readonly UnitReference NullUnitReference = new UnitReference((string)null);

	[OwlPackInclude]
	private readonly string m_UniqueId;

	private EntityServiceProxy m_Proxy;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitReference",
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_UniqueId", typeof(string))
		}
	};

	public string Id => m_UniqueId;

	public IAbstractUnitEntity Entity
	{
		get
		{
			if ((m_Proxy == null || m_Proxy.IsDisposed) && !m_UniqueId.IsNullOrEmpty())
			{
				m_Proxy = EntityService.Instance?.GetProxy(m_UniqueId);
			}
			return m_Proxy?.Entity as IAbstractUnitEntity;
		}
	}

	public UnitReference([CanBeNull] string m_uniqueId)
	{
		m_UniqueId = m_uniqueId;
		m_Proxy = (string.IsNullOrEmpty(m_uniqueId) ? null : EntityService.Instance?.GetProxy(m_uniqueId));
	}

	private UnitReference([CanBeNull] IAbstractUnitEntity unit)
	{
		m_UniqueId = unit?.UniqueId;
		m_Proxy = unit?.Proxy;
	}

	[CanBeNull]
	public T Get<T>() where T : class, IEntity
	{
		return (T)Entity;
	}

	[CanBeNull]
	public IEntity Get()
	{
		return Entity;
	}

	public bool IsNull()
	{
		return Entity == null;
	}

	public bool Equals(UnitReference other)
	{
		return string.Equals(Id, other.Id, StringComparison.Ordinal);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is UnitReference other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (m_UniqueId == null)
		{
			return 0;
		}
		return m_UniqueId.GetHashCode();
	}

	public IAbstractUnitEntity ToIAbstractUnitEntity()
	{
		return Entity;
	}

	public static IAbstractUnitEntity ToIAbstractUnitEntity(UnitReference r)
	{
		if (!(r == null))
		{
			return r.Entity;
		}
		return null;
	}

	public static UnitReference FromIAbstractUnitEntity(IAbstractUnitEntity unit)
	{
		return new UnitReference(unit);
	}

	public static bool operator ==(UnitReference r, UnitReference l)
	{
		return string.Equals(r.Id, l.Id);
	}

	public static bool operator !=(UnitReference r, UnitReference l)
	{
		return !string.Equals(r.Id, l.Id);
	}

	public static bool operator ==(UnitReference r, [CanBeNull] IAbstractUnitEntity unit)
	{
		return string.Equals(r.Id, unit?.UniqueId, StringComparison.Ordinal);
	}

	public static bool operator !=(UnitReference r, [CanBeNull] IAbstractUnitEntity unit)
	{
		return !string.Equals(r.Id, unit?.UniqueId, StringComparison.Ordinal);
	}

	public override string ToString()
	{
		if (Entity == null)
		{
			if (m_UniqueId == null)
			{
				return "<null>";
			}
			return "[Not found] " + m_UniqueId;
		}
		return Entity.CharacterName;
	}

	public int CompareTo(UnitReference other)
	{
		return string.Compare(m_UniqueId, other.m_UniqueId, StringComparison.Ordinal);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitReference source = default(UnitReference);
		result = Unsafe.As<UnitReference, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitReference>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = m_UniqueId;
		formatter.StringField(0, "m_UniqueId", ref value, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitReference>();
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
				Unsafe.AsRef(in m_UniqueId) = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
