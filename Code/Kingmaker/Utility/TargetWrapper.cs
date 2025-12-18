using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Utility;

[OwlPackable(OwlPackableMode.Generate)]
public class TargetWrapper : ITargetWrapper, IHashable, IOwlPackable, IOwlPackable<TargetWrapper>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "TargetWrapper",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("EntityRef", typeof(EntityRef<MechanicEntity>)),
			new FieldInfo("m_Point", typeof(Vector3?)),
			new FieldInfo("m_Orientation", typeof(float?))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public EntityRef<MechanicEntity> EntityRef { get; protected set; }

	[JsonProperty]
	[OwlPackInclude]
	protected Vector3? m_Point { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	protected float? m_Orientation { get; set; }

	[CanBeNull]
	public MechanicEntity Entity => EntityRef.Entity;

	[CanBeNull]
	public IMechanicEntity IEntity => EntityRef.Entity;

	public virtual Vector3 Point => m_Point ?? Entity?.Position ?? default(Vector3);

	public virtual float Orientation
	{
		get
		{
			float? orientation = m_Orientation;
			if (!orientation.HasValue)
			{
				if (!IsPoint)
				{
					return Entity?.Orientation ?? 0f;
				}
				return 0f;
			}
			return orientation.GetValueOrDefault();
		}
	}

	public bool HasEntity => !EntityRef.IsNull;

	public bool IsPoint => m_Point.HasValue;

	public virtual bool IsOrientationSpecified => m_Orientation.HasValue;

	public IntRect SizeRect
	{
		get
		{
			if (!IsPoint)
			{
				return Entity?.SizeRect ?? default(IntRect);
			}
			return default(IntRect);
		}
	}

	public Vector3 Forward
	{
		get
		{
			if (!IsPoint)
			{
				return Entity?.Forward ?? Vector3.forward;
			}
			return Vector3.forward;
		}
	}

	public GridNodeBase NearestNode => Point.GetNearestNodeXZUnwalkable();

	public TargetWrapper([NotNull] MechanicEntity unit)
	{
		EntityRef = unit ?? throw new ArgumentException("TargetWrapper: 'unit' is null");
	}

	public TargetWrapper(Vector3 point, float? orientation = null, MechanicEntity entity = null)
	{
		m_Point = point;
		m_Orientation = orientation;
		EntityRef = entity;
	}

	protected TargetWrapper([NotNull] TargetWrapper other)
	{
		m_Point = other.m_Point;
		m_Orientation = other.m_Orientation;
		EntityRef = other.EntityRef;
	}

	[JsonConstructor]
	protected TargetWrapper()
	{
	}

	public static implicit operator TargetWrapper(MechanicEntity unit)
	{
		if (unit == null)
		{
			return null;
		}
		return new TargetWrapper(unit);
	}

	public static implicit operator TargetWrapper(Vector3 point)
	{
		return new TargetWrapper(point);
	}

	public static implicit operator TargetWrapper(Vector3? point)
	{
		if (!point.HasValue)
		{
			return null;
		}
		return new TargetWrapper(point.Value);
	}

	public override string ToString()
	{
		if (!HasEntity)
		{
			return $"[Target: point '{Point}']";
		}
		return $"[Target: unit '{Entity}' {Point}]";
	}

	public override bool Equals(object obj)
	{
		return Equals(this, obj as TargetWrapper);
	}

	public virtual bool Equals(TargetWrapper other)
	{
		return Equals(this, other);
	}

	public static bool operator ==(TargetWrapper t1, TargetWrapper t2)
	{
		return Equals(t1, t2);
	}

	public static bool operator !=(TargetWrapper t1, TargetWrapper t2)
	{
		return !Equals(t1, t2);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(EntityRef, m_Point, m_Orientation);
	}

	protected static bool Equals(TargetWrapper x, TargetWrapper y)
	{
		if ((object)x == y)
		{
			return true;
		}
		if ((object)x == null)
		{
			return false;
		}
		if ((object)y == null)
		{
			return false;
		}
		if (x.GetType() != y.GetType())
		{
			return false;
		}
		if (x.EntityRef.Equals(y.EntityRef) && Nullable.Equals(x.m_Point, y.m_Point))
		{
			return Nullable.Equals(x.m_Orientation, y.m_Orientation);
		}
		return false;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<MechanicEntity> obj = EntityRef;
		Hash128 val = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		if (m_Point.HasValue)
		{
			Vector3 val2 = m_Point.Value;
			result.Append(ref val2);
		}
		if (m_Orientation.HasValue)
		{
			float val3 = m_Orientation.Value;
			result.Append(ref val3);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		TargetWrapper source = new TargetWrapper();
		result = Unsafe.As<TargetWrapper, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<TargetWrapper>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityRef<MechanicEntity> value = EntityRef;
		formatter.Field(0, "EntityRef", ref value, state);
		Vector3? value2 = m_Point;
		formatter.NullableField(1, "m_Point", ref value2, state);
		float? value3 = m_Orientation;
		formatter.UnmanagedNullableField(2, "m_Orientation", ref value3, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<TargetWrapper>();
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
				EntityRef = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
				break;
			case 1:
				m_Point = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 2:
				m_Orientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
