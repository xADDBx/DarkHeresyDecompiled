using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

[OwlPackable(OwlPackableMode.Generate)]
public class RespecInfo : IHashable, IOwlPackable, IOwlPackable<RespecInfo>
{
	[JsonProperty]
	[OwlPackInclude]
	private int m_RespecCount;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_HasExtraRespec;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "RespecInfo",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_RespecCount", typeof(int)),
			new FieldInfo("m_HasExtraRespec", typeof(bool))
		}
	};

	public int GetRespecCost()
	{
		if (m_HasExtraRespec)
		{
			return 0;
		}
		if (m_RespecCount >= 3)
		{
			return m_RespecCount - 2;
		}
		return 0;
	}

	public void CountRespecIn()
	{
		if (m_HasExtraRespec)
		{
			m_HasExtraRespec = false;
		}
		else
		{
			m_RespecCount++;
		}
	}

	public void GiveExtraRespec()
	{
		m_HasExtraRespec = true;
	}

	public RespecInfo()
	{
		m_RespecCount = 0;
		m_HasExtraRespec = false;
	}

	public RespecInfo(int respecsHappened)
	{
		m_RespecCount = respecsHappened;
		m_HasExtraRespec = false;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref m_RespecCount);
		result.Append(ref m_HasExtraRespec);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		RespecInfo source = new RespecInfo();
		result = Unsafe.As<RespecInfo, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<RespecInfo>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "m_RespecCount", ref m_RespecCount, state);
		formatter.UnmanagedField(1, "m_HasExtraRespec", ref m_HasExtraRespec, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<RespecInfo>();
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
				m_RespecCount = formatter.ReadUnmanaged<int>(state);
				break;
			case 1:
				m_HasExtraRespec = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
