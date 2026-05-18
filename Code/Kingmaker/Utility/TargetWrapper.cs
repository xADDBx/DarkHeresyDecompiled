using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Utility;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class TargetWrapper : ITargetWrapper, IMemoryPackable<TargetWrapper>, IMemoryPackFormatterRegister, IHashable, IOwlPackable, IOwlPackable<TargetWrapper>
{
	[Preserve]
	private sealed class TargetWrapperFormatter : MemoryPackFormatter<TargetWrapper>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref TargetWrapper value)
		{
			TargetWrapper.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref TargetWrapper value)
		{
			TargetWrapper.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref TargetWrapper value)
		{
			TargetWrapper.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref TargetWrapper value)
		{
			TargetWrapper.DeserializeJson(ref reader, ref value);
		}
	}

	public static readonly TypeInfo OwlPackTypeInfo;

	[JsonProperty]
	[OwlPackInclude]
	public EntityRef<MechanicEntity> EntityRef { get; protected set; }

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	protected Vector3? m_Point { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	protected float? m_Orientation { get; set; }

	[CanBeNull]
	[MemoryPackIgnore]
	public MechanicEntity Entity => EntityRef.Entity;

	[CanBeNull]
	[MemoryPackIgnore]
	public IMechanicEntity IEntity => EntityRef.Entity;

	[MemoryPackIgnore]
	public virtual Vector3 Point => m_Point ?? Entity?.Position ?? default(Vector3);

	[MemoryPackIgnore]
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

	[MemoryPackIgnore]
	public bool HasEntity => !EntityRef.IsNull;

	[MemoryPackIgnore]
	public bool IsPoint => m_Point.HasValue;

	[MemoryPackIgnore]
	public virtual bool IsOrientationSpecified => m_Orientation.HasValue;

	[MemoryPackIgnore]
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

	[MemoryPackIgnore]
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

	[MemoryPackIgnore]
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

	[MemoryPackConstructor]
	protected TargetWrapper(EntityRef<MechanicEntity> entityRef, Vector3? m_point, float? m_orientation)
	{
		EntityRef = entityRef;
		m_Point = m_point;
		m_Orientation = m_orientation;
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

	static TargetWrapper()
	{
		OwlPackTypeInfo = new TypeInfo
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
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<TargetWrapper>())
		{
			MemoryPackFormatterProvider.Register(new TargetWrapperFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<TargetWrapper[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<TargetWrapper>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Vector3?>())
		{
			MemoryPackFormatterProvider.Register(new NullableFormatter<Vector3>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<float?>())
		{
			MemoryPackFormatterProvider.Register(new NullableFormatter<float>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref TargetWrapper? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		EntityRef<MechanicEntity> value2 = value.EntityRef;
		writer.WritePackable(in value2);
		Vector3? value3 = value.m_Point;
		float? value4 = value.m_Orientation;
		writer.DangerousWriteUnmanaged(in value3, in value4);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref TargetWrapper? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<MechanicEntity> value2;
		Vector3? value3;
		float? value4;
		if (memberCount == 3)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<MechanicEntity>>();
				reader.DangerousReadUnmanaged<Vector3?, float?>(out value3, out value4);
			}
			else
			{
				value2 = value.EntityRef;
				value3 = value.m_Point;
				value4 = value.m_Orientation;
				reader.ReadPackable(ref value2);
				reader.DangerousReadUnmanaged<Vector3?>(out value3);
				reader.DangerousReadUnmanaged<float?>(out value4);
			}
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(TargetWrapper), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<MechanicEntity>);
				value3 = null;
				value4 = null;
			}
			else
			{
				value2 = value.EntityRef;
				value3 = value.m_Point;
				value4 = value.m_Orientation;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.DangerousReadUnmanaged<Vector3?>(out value3);
					if (memberCount != 2)
					{
						reader.DangerousReadUnmanaged<float?>(out value4);
						_ = 3;
					}
				}
			}
			_ = value == null;
		}
		value = new TargetWrapper(value2, value3, value4);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref TargetWrapper? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("EntityRef");
		writer.WritePackable(value.EntityRef);
		writer.WriteProperty("m_Point");
		writer.DangerousWriteUnmanaged(value.m_Point);
		writer.WriteProperty("m_Orientation");
		writer.DangerousWriteUnmanaged(value.m_Orientation);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref TargetWrapper? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityRef<MechanicEntity> val;
		Vector3? v;
		float? v2;
		if (value == null)
		{
			val = default(EntityRef<MechanicEntity>);
			v = null;
			v2 = null;
		}
		else
		{
			val = value.EntityRef;
			v = value.m_Point;
			v2 = value.m_Orientation;
		}
		bool[] array = new bool[3];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "EntityRef":
					val = reader.ReadPackable<EntityRef<MechanicEntity>>();
					array[0] = true;
					break;
				case "m_Point":
					reader.DangerousReadUnmanaged<Vector3?>(out v);
					array[1] = true;
					break;
				case "m_Orientation":
					reader.DangerousReadUnmanaged<float?>(out v2);
					array[2] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "EntityRef":
					reader.ReadPackable(ref val);
					break;
				case "m_Point":
					reader.DangerousReadUnmanaged<Vector3?>(out v);
					break;
				case "m_Orientation":
					reader.DangerousReadUnmanaged<float?>(out v2);
					break;
				}
			}
		}
		_ = value == null;
		value = new TargetWrapper(val, v, v2);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
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
