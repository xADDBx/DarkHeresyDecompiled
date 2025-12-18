using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Networking.Serialization;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UI;

[JsonObject]
[OwlPackable(OwlPackableMode.Generate)]
public class SelectedConclusions : IHashable, IOwlPackable, IOwlPackable<SelectedConclusions>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SelectedConclusions",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_TypesToConclusions", typeof(Dictionary<BlueprintConclusionType, BlueprintConclusion>))
		}
	};

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	private Dictionary<BlueprintConclusionType, BlueprintConclusion> m_TypesToConclusions { get; set; } = new Dictionary<BlueprintConclusionType, BlueprintConclusion>();


	public bool TryGetClueForType(BlueprintConclusionType type, out BlueprintConclusion conclusion)
	{
		return m_TypesToConclusions.TryGetValue(type, out conclusion);
	}

	public void SetConclusionForType(BlueprintConclusionType type, BlueprintConclusion conclusion)
	{
		if (!m_TypesToConclusions.TryAdd(type, conclusion))
		{
			m_TypesToConclusions[type] = conclusion;
		}
	}

	public bool IsSelected(BlueprintConclusion conclusion)
	{
		return m_TypesToConclusions.ContainsValue(conclusion);
	}

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SelectedConclusions source = new SelectedConclusions();
		result = Unsafe.As<SelectedConclusions, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SelectedConclusions>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		Dictionary<BlueprintConclusionType, BlueprintConclusion> value = m_TypesToConclusions;
		formatter.Field(0, "m_TypesToConclusions", ref value, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SelectedConclusions>();
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
				m_TypesToConclusions = formatter.ReadPackable<Dictionary<BlueprintConclusionType, BlueprintConclusion>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
