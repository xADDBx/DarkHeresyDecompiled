using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UI;

[JsonObject]
[OwlPackable(OwlPackableMode.Generate)]
public class DetectiveSystemData : IHashable, IOwlPackable, IOwlPackable<DetectiveSystemData>
{
	[JsonProperty]
	[OwlPackInclude]
	public CustomCluesConnections CluesConnections = new CustomCluesConnections();

	[JsonProperty]
	[OwlPackInclude]
	public SelectedConclusions SelectedConclusions = new SelectedConclusions();

	[JsonProperty]
	[OwlPackInclude]
	public ExaminedDetectiveData ExaminedDetectiveData = new ExaminedDetectiveData();

	[JsonProperty]
	[OwlPackInclude]
	public JournalPositions<BlueprintClue> CluesPositions = new JournalPositions<BlueprintClue>();

	[JsonProperty]
	[OwlPackInclude]
	public JournalPositions<BlueprintConclusion> ConclusionPositions = new JournalPositions<BlueprintConclusion>();

	[JsonProperty]
	[OwlPackInclude]
	public Dictionary<BlueprintClueStudy, int> StudyIds = new Dictionary<BlueprintClueStudy, int>();

	[JsonProperty]
	[OwlPackInclude]
	public bool ShowClosedCases = true;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "DetectiveSystemData",
		OldNames = null,
		Fields = new FieldInfo[7]
		{
			new FieldInfo("CluesConnections", typeof(CustomCluesConnections)),
			new FieldInfo("SelectedConclusions", typeof(SelectedConclusions)),
			new FieldInfo("ExaminedDetectiveData", typeof(ExaminedDetectiveData)),
			new FieldInfo("CluesPositions", typeof(JournalPositions<BlueprintClue>)),
			new FieldInfo("ConclusionPositions", typeof(JournalPositions<BlueprintConclusion>)),
			new FieldInfo("StudyIds", typeof(Dictionary<BlueprintClueStudy, int>)),
			new FieldInfo("ShowClosedCases", typeof(bool))
		}
	};

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = ClassHasher<CustomCluesConnections>.GetHash128(CluesConnections);
		result.Append(ref val);
		Hash128 val2 = ClassHasher<SelectedConclusions>.GetHash128(SelectedConclusions);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<ExaminedDetectiveData>.GetHash128(ExaminedDetectiveData);
		result.Append(ref val3);
		Hash128 val4 = ClassHasher<JournalPositions<BlueprintClue>>.GetHash128(CluesPositions);
		result.Append(ref val4);
		Hash128 val5 = ClassHasher<JournalPositions<BlueprintConclusion>>.GetHash128(ConclusionPositions);
		result.Append(ref val5);
		Dictionary<BlueprintClueStudy, int> studyIds = StudyIds;
		if (studyIds != null)
		{
			int val6 = 0;
			foreach (KeyValuePair<BlueprintClueStudy, int> item in studyIds)
			{
				Hash128 hash = default(Hash128);
				Hash128 val7 = SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val7);
				int obj = item.Value;
				Hash128 val8 = UnmanagedHasher<int>.GetHash128(ref obj);
				hash.Append(ref val8);
				val6 ^= hash.GetHashCode();
			}
			result.Append(ref val6);
		}
		result.Append(ref ShowClosedCases);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		DetectiveSystemData source = new DetectiveSystemData();
		result = Unsafe.As<DetectiveSystemData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<DetectiveSystemData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "CluesConnections", ref CluesConnections, state);
		formatter.Field(1, "SelectedConclusions", ref SelectedConclusions, state);
		formatter.Field(2, "ExaminedDetectiveData", ref ExaminedDetectiveData, state);
		formatter.Field(3, "CluesPositions", ref CluesPositions, state);
		formatter.Field(4, "ConclusionPositions", ref ConclusionPositions, state);
		formatter.Field(5, "StudyIds", ref StudyIds, state);
		formatter.UnmanagedField(6, "ShowClosedCases", ref ShowClosedCases, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DetectiveSystemData>();
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
				CluesConnections = formatter.ReadPackable<CustomCluesConnections>(state);
				break;
			case 1:
				SelectedConclusions = formatter.ReadPackable<SelectedConclusions>(state);
				break;
			case 2:
				ExaminedDetectiveData = formatter.ReadPackable<ExaminedDetectiveData>(state);
				break;
			case 3:
				CluesPositions = formatter.ReadPackable<JournalPositions<BlueprintClue>>(state);
				break;
			case 4:
				ConclusionPositions = formatter.ReadPackable<JournalPositions<BlueprintConclusion>>(state);
				break;
			case 5:
				StudyIds = formatter.ReadPackable<Dictionary<BlueprintClueStudy, int>>(state);
				break;
			case 6:
				ShowClosedCases = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
