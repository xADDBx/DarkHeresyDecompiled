using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Utility.FlagCountable;

[OwlPackable(OwlPackableMode.Generate)]
[HashRoot]
public class CountableFlag : IHashable, IOwlPackable, IOwlPackable<CountableFlag>
{
	[JsonProperty]
	[OwlPackInclude]
	private int m_Count;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CountableFlag",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Count", typeof(int))
		}
	};

	public int Count => m_Count;

	public bool Value => this;

	public void Retain()
	{
		m_Count++;
	}

	public void Release()
	{
		if (Application.isEditor && m_Count < 1)
		{
			PFLog.Default.Error("Can't release countable flag: no one retain it");
		}
		m_Count = Math.Max(0, m_Count - 1);
	}

	public void ReleaseAll()
	{
		m_Count = 0;
	}

	public static implicit operator bool(CountableFlag flag)
	{
		if (flag != null)
		{
			return flag.m_Count > 0;
		}
		return false;
	}

	public override string ToString()
	{
		return $"{(bool)this}({m_Count})";
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref m_Count);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CountableFlag source = new CountableFlag();
		result = Unsafe.As<CountableFlag, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CountableFlag>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "m_Count", ref m_Count, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CountableFlag>();
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
				m_Count = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
