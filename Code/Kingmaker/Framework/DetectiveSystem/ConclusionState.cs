using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Networking.Serialization;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Framework.DetectiveSystem;

[OwlPackable(OwlPackableMode.Generate)]
[OwlPackOldName("Kingmaker.Framework.DetectiveSystem.DetectiveSystem+ConclusionState")]
internal sealed class ConclusionState : CaseItemState, IHashable, IOwlPackable<ConclusionState>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ConclusionState",
		OldNames = new string[1] { "Kingmaker.Framework.DetectiveSystem.DetectiveSystem+ConclusionState" },
		Fields = new FieldInfo[2]
		{
			new FieldInfo("Hidden", typeof(bool)),
			new FieldInfo("Made", typeof(bool))
		}
	};

	[OwlPackInclude]
	[GameStateInclude]
	public bool Made { get; set; }

	public ConclusionState(BlueprintConclusion _)
	{
	}

	[UsedImplicitly]
	private ConclusionState(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = Made;
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ConclusionState source = new ConclusionState(default(OwlPackConstructorParameter));
		result = Unsafe.As<ConclusionState, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ConclusionState>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		bool value = base.Hidden;
		formatter.UnmanagedField(0, "Hidden", ref value, state);
		bool value2 = Made;
		formatter.UnmanagedField(1, "Made", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ConclusionState>();
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
				base.Hidden = formatter.ReadUnmanaged<bool>(state);
				break;
			case 1:
				Made = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
