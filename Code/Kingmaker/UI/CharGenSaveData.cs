using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Networking.Serialization;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UI;

[Serializable]
[JsonObject]
[OwlPackable(OwlPackableMode.Generate)]
public class CharGenSaveData : IHashable, IOwlPackable, IOwlPackable<CharGenSaveData>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CharGenSaveData",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_UnitsData", typeof(Dictionary<string, CharGenUnitSaveData>))
		}
	};

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	private Dictionary<string, CharGenUnitSaveData> m_UnitsData { get; set; } = new Dictionary<string, CharGenUnitSaveData>();


	public CharGenUnitSaveData GetDataForUnit(string guid)
	{
		if (!m_UnitsData.TryGetValue(guid, out var value))
		{
			value = new CharGenUnitSaveData();
			m_UnitsData[guid] = value;
		}
		return value;
	}

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSaveData source = new CharGenSaveData();
		result = Unsafe.As<CharGenSaveData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSaveData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		Dictionary<string, CharGenUnitSaveData> value = m_UnitsData;
		formatter.Field(0, "m_UnitsData", ref value, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSaveData>();
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
				m_UnitsData = formatter.ReadPackable<Dictionary<string, CharGenUnitSaveData>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
