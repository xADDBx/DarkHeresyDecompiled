using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Controllers.Projectiles;

[OwlPackable(OwlPackableMode.Generate)]
public class ProjectileTargetWrapper : TargetWrapper, IHashable, IOwlPackable<ProjectileTargetWrapper>
{
	private readonly Transform m_Transform;

	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ProjectileTargetWrapper",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("EntityRef", typeof(EntityRef<MechanicEntity>)),
			new FieldInfo("m_Point", typeof(Vector3?)),
			new FieldInfo("m_Orientation", typeof(float?))
		}
	};

	public override Vector3 Point => m_Transform.Or(null)?.position ?? base.Point;

	public override float Orientation => m_Transform.Or(null)?.rotation.y ?? base.Orientation;

	public override bool IsOrientationSpecified => m_Transform != null;

	public override string ToString()
	{
		if ((bool)m_Transform)
		{
			return "[Target: point '" + m_Transform.name + "']";
		}
		return base.ToString();
	}

	protected ProjectileTargetWrapper()
	{
	}

	public ProjectileTargetWrapper([NotNull] Transform t)
	{
		m_Transform = t.Or(null) ?? throw new ArgumentException("TargetWrapper: 'transform' is null");
	}

	public ProjectileTargetWrapper([NotNull] BaseUnitEntity unit)
		: base(unit)
	{
	}

	public ProjectileTargetWrapper(Vector3 point, float orientation)
		: base(point, orientation)
	{
	}

	public static implicit operator ProjectileTargetWrapper([NotNull] Transform t)
	{
		return new ProjectileTargetWrapper(t);
	}

	public static implicit operator ProjectileTargetWrapper([NotNull] BaseUnitEntity unit)
	{
		return new ProjectileTargetWrapper(unit);
	}

	public static implicit operator ProjectileTargetWrapper(Vector3 point)
	{
		return new ProjectileTargetWrapper(point, 0f);
	}

	public override bool Equals(object obj)
	{
		return Equals(this, obj as ProjectileTargetWrapper);
	}

	public override bool Equals(TargetWrapper other)
	{
		return Equals(this, other as ProjectileTargetWrapper);
	}

	public bool Equals(ProjectileTargetWrapper other)
	{
		return Equals(this, other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), m_Transform.GetHashCode());
	}

	private static bool Equals(ProjectileTargetWrapper x, ProjectileTargetWrapper y)
	{
		if (!TargetWrapper.Equals(x, y))
		{
			return false;
		}
		return x.m_Transform.Equals(y.m_Transform);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ProjectileTargetWrapper source = new ProjectileTargetWrapper();
		result = Unsafe.As<ProjectileTargetWrapper, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<ProjectileTargetWrapper>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityRef<MechanicEntity> value = base.EntityRef;
		formatter.Field(0, "EntityRef", ref value, state);
		Vector3? value2 = base.m_Point;
		formatter.NullableField(1, "m_Point", ref value2, state);
		float? value3 = base.m_Orientation;
		formatter.UnmanagedNullableField(2, "m_Orientation", ref value3, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ProjectileTargetWrapper>();
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
				base.EntityRef = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
				break;
			case 1:
				base.m_Point = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 2:
				base.m_Orientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
