using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Localization;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

[Serializable]
[OwlPackable(OwlPackableMode.Generate)]
public class AlignmentShiftHistoryEntry : IHashable, IOwlPackable, IOwlPackable<AlignmentShiftHistoryEntry>
{
	[JsonProperty]
	[OwlPackInclude]
	public AlignmentAxis Axis;

	[JsonProperty]
	[OwlPackInclude]
	public BlueprintScriptableObject Source;

	[JsonProperty]
	[OwlPackInclude]
	public int Rank;

	[JsonProperty]
	[OwlPackInclude]
	public bool AchievedNewMark;

	[JsonProperty]
	[OwlPackInclude]
	public List<BlueprintMechanicEntityFact> NewFacts = new List<BlueprintMechanicEntityFact>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AlignmentShiftHistoryEntry",
		OldNames = null,
		Fields = new FieldInfo[5]
		{
			new FieldInfo("Axis", typeof(AlignmentAxis)),
			new FieldInfo("Source", typeof(BlueprintScriptableObject)),
			new FieldInfo("Rank", typeof(int)),
			new FieldInfo("AchievedNewMark", typeof(bool)),
			new FieldInfo("NewFacts", typeof(List<BlueprintMechanicEntityFact>))
		}
	};

	public LocalizedString Description
	{
		get
		{
			if (!(Source is IAlignmentShiftProvider alignmentShiftProvider))
			{
				return null;
			}
			return alignmentShiftProvider.AlignmentShifts.FirstOrDefault()?.Description;
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref Axis);
		Hash128 val = SimpleBlueprintHasher.GetHash128(Source);
		result.Append(ref val);
		result.Append(ref Rank);
		result.Append(ref AchievedNewMark);
		List<BlueprintMechanicEntityFact> newFacts = NewFacts;
		if (newFacts != null)
		{
			for (int i = 0; i < newFacts.Count; i++)
			{
				Hash128 val2 = SimpleBlueprintHasher.GetHash128(newFacts[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		AlignmentShiftHistoryEntry source = new AlignmentShiftHistoryEntry();
		result = Unsafe.As<AlignmentShiftHistoryEntry, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AlignmentShiftHistoryEntry>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EnumField(0, "Axis", ref Axis, state);
		formatter.Field(1, "Source", ref Source, state);
		formatter.UnmanagedField(2, "Rank", ref Rank, state);
		formatter.UnmanagedField(3, "AchievedNewMark", ref AchievedNewMark, state);
		formatter.Field(4, "NewFacts", ref NewFacts, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AlignmentShiftHistoryEntry>();
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
				Axis = formatter.ReadEnum<AlignmentAxis>(state);
				break;
			case 1:
				Source = formatter.ReadPackable<BlueprintScriptableObject>(state);
				break;
			case 2:
				Rank = formatter.ReadUnmanaged<int>(state);
				break;
			case 3:
				AchievedNewMark = formatter.ReadUnmanaged<bool>(state);
				break;
			case 4:
				NewFacts = formatter.ReadPackable<List<BlueprintMechanicEntityFact>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
