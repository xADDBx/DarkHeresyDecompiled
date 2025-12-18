using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Utility;

[Serializable]
[JsonObject(IsReference = false)]
[HashRoot]
[OwlPackable(OwlPackableMode.Generate)]
public struct Rounds : IHashable, IOwlPackable, IOwlPackable<Rounds>
{
	public static readonly Rounds Infinity = new Rounds(int.MaxValue);

	[SerializeField]
	[HideInInspector]
	[JsonProperty]
	[OwlPackInclude]
	private int m_Value;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "Rounds",
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Value", typeof(int))
		}
	};

	public int Value => m_Value;

	public TimeSpan Seconds => ((float)m_Value * 5f).Seconds();

	public Rounds(int value)
	{
		m_Value = value;
	}

	public bool Equals(Rounds other)
	{
		return m_Value == other.m_Value;
	}

	public override bool Equals(object obj)
	{
		if (obj is Rounds other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_Value;
	}

	public override string ToString()
	{
		return m_Value.ToString();
	}

	public static Rounds operator +(Rounds v1, Rounds v2)
	{
		return new Rounds(v1.m_Value + v2.m_Value);
	}

	public static Rounds operator -(Rounds v1, Rounds v2)
	{
		return new Rounds(v1.m_Value - v2.m_Value);
	}

	public static Rounds operator *(Rounds v1, int multiplier)
	{
		return new Rounds(v1.m_Value * multiplier);
	}

	public static Rounds operator /(Rounds v1, int divider)
	{
		return new Rounds(v1.m_Value / divider);
	}

	public static bool operator ==(Rounds v1, Rounds v2)
	{
		return v1.Equals(v2);
	}

	public static bool operator !=(Rounds v1, Rounds v2)
	{
		return !(v1 == v2);
	}

	public static bool operator <=(Rounds v1, Rounds v2)
	{
		return v1.m_Value <= v2.m_Value;
	}

	public static bool operator >=(Rounds v1, Rounds v2)
	{
		return v1.m_Value >= v2.m_Value;
	}

	public static bool operator <(Rounds v1, Rounds v2)
	{
		return v1.m_Value < v2.m_Value;
	}

	public static bool operator >(Rounds v1, Rounds v2)
	{
		return v1.m_Value > v2.m_Value;
	}

	public static Rounds Max(Rounds v1, Rounds v2)
	{
		if (!(v1 > v2))
		{
			return v2;
		}
		return v1;
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref m_Value);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		Rounds source = default(Rounds);
		result = Unsafe.As<Rounds, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<Rounds>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "m_Value", ref m_Value, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Rounds>();
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
				m_Value = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
