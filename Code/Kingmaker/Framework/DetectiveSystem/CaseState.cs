using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Networking.Serialization;
using Kingmaker.StateHasher.Hashers;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Framework.DetectiveSystem;

[HashRoot]
[OwlPackable(OwlPackableMode.Generate)]
[OwlPackOldName("Kingmaker.Framework.DetectiveSystem.DetectiveSystem+CaseState")]
internal sealed class CaseState : IHashable, IOwlPackable, IOwlPackable<CaseState>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CaseState",
		OldNames = new string[1] { "Kingmaker.Framework.DetectiveSystem.DetectiveSystem+CaseState" },
		Fields = new FieldInfo[6]
		{
			new FieldInfo("Status", typeof(CaseStatus)),
			new FieldInfo("Question", typeof(BlueprintCaseQuestion)),
			new FieldInfo("Answer", typeof(BlueprintCaseAnswer)),
			new FieldInfo("OpenedByCheats", typeof(bool)),
			new FieldInfo("CorrectConclusionsCount", typeof(int)),
			new FieldInfo("Conclusions", typeof(BlueprintConclusion[]))
		}
	};

	[OwlPackInclude]
	[GameStateInclude]
	public CaseStatus Status { get; set; }

	[OwlPackInclude]
	[GameStateInclude]
	[CanBeNull]
	public BlueprintCaseQuestion Question { get; set; }

	[OwlPackInclude]
	[GameStateInclude]
	[CanBeNull]
	public BlueprintCaseAnswer Answer { get; set; }

	[OwlPackInclude]
	[GameStateInclude]
	public bool OpenedByCheats { get; set; }

	[Obsolete("New Question/Answer approach, WIP")]
	[OwlPackInclude]
	public int CorrectConclusionsCount { get; set; }

	[Obsolete("New Question/Answer approach, WIP")]
	[OwlPackInclude]
	[CanBeNull]
	public BlueprintConclusion[] Conclusions { get; set; }

	public CaseState(BlueprintCase _)
	{
	}

	[UsedImplicitly]
	private CaseState(OwlPackConstructorParameter _)
	{
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		CaseStatus val = Status;
		result.Append(ref val);
		Hash128 val2 = SimpleBlueprintHasher.GetHash128(Question);
		result.Append(ref val2);
		Hash128 val3 = SimpleBlueprintHasher.GetHash128(Answer);
		result.Append(ref val3);
		bool val4 = OpenedByCheats;
		result.Append(ref val4);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CaseState source = new CaseState(default(OwlPackConstructorParameter));
		result = Unsafe.As<CaseState, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CaseState>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		CaseStatus value = Status;
		formatter.EnumField(0, "Status", ref value, state);
		BlueprintCaseQuestion value2 = Question;
		formatter.Field(1, "Question", ref value2, state);
		BlueprintCaseAnswer value3 = Answer;
		formatter.Field(2, "Answer", ref value3, state);
		bool value4 = OpenedByCheats;
		formatter.UnmanagedField(3, "OpenedByCheats", ref value4, state);
		int value5 = CorrectConclusionsCount;
		formatter.UnmanagedField(4, "CorrectConclusionsCount", ref value5, state);
		BlueprintConclusion[] value6 = Conclusions;
		formatter.Field(5, "Conclusions", ref value6, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CaseState>();
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
				Status = formatter.ReadEnum<CaseStatus>(state);
				break;
			case 1:
				Question = formatter.ReadPackable<BlueprintCaseQuestion>(state);
				break;
			case 2:
				Answer = formatter.ReadPackable<BlueprintCaseAnswer>(state);
				break;
			case 3:
				OpenedByCheats = formatter.ReadUnmanaged<bool>(state);
				break;
			case 4:
				CorrectConclusionsCount = formatter.ReadUnmanaged<int>(state);
				break;
			case 5:
				Conclusions = formatter.ReadPackable<BlueprintConclusion[]>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
