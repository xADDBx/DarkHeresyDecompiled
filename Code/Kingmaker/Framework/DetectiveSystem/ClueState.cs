using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Networking.Serialization;
using Kingmaker.StateHasher.Hashers;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Framework.DetectiveSystem;

[OwlPackable(OwlPackableMode.Generate)]
[OwlPackOldName("Kingmaker.Framework.DetectiveSystem.DetectiveSystem+ClueState")]
internal sealed class ClueState : CaseItemState, IHashable, IOwlPackable<ClueState>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ClueState",
		OldNames = new string[1] { "Kingmaker.Framework.DetectiveSystem.DetectiveSystem+ClueState" },
		Fields = new FieldInfo[5]
		{
			new FieldInfo("Hidden", typeof(bool)),
			new FieldInfo("Source", typeof(BlueprintScriptableObject)),
			new FieldInfo("PlaceOfIssue", typeof(BlueprintArea)),
			new FieldInfo("Found", typeof(bool)),
			new FieldInfo("Observed", typeof(bool))
		}
	};

	[OwlPackInclude]
	[GameStateInclude]
	[CanBeNull]
	public BlueprintScriptableObject Source { get; set; }

	[OwlPackInclude]
	[GameStateInclude]
	[CanBeNull]
	public BlueprintArea PlaceOfIssue { get; set; }

	[OwlPackInclude]
	[GameStateInclude]
	public bool Found { get; set; }

	[OwlPackInclude]
	[GameStateInclude]
	public bool Observed { get; set; }

	public ClueState(BlueprintClue _)
	{
	}

	[UsedImplicitly]
	private ClueState(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = SimpleBlueprintHasher.GetHash128(Source);
		result.Append(ref val2);
		Hash128 val3 = SimpleBlueprintHasher.GetHash128(PlaceOfIssue);
		result.Append(ref val3);
		bool val4 = Found;
		result.Append(ref val4);
		bool val5 = Observed;
		result.Append(ref val5);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ClueState source = new ClueState(default(OwlPackConstructorParameter));
		result = Unsafe.As<ClueState, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ClueState>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		bool value = base.Hidden;
		formatter.UnmanagedField(0, "Hidden", ref value, state);
		BlueprintScriptableObject value2 = Source;
		formatter.Field(1, "Source", ref value2, state);
		BlueprintArea value3 = PlaceOfIssue;
		formatter.Field(2, "PlaceOfIssue", ref value3, state);
		bool value4 = Found;
		formatter.UnmanagedField(3, "Found", ref value4, state);
		bool value5 = Observed;
		formatter.UnmanagedField(4, "Observed", ref value5, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ClueState>();
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
				Source = formatter.ReadPackable<BlueprintScriptableObject>(state);
				break;
			case 2:
				PlaceOfIssue = formatter.ReadPackable<BlueprintArea>(state);
				break;
			case 3:
				Found = formatter.ReadUnmanaged<bool>(state);
				break;
			case 4:
				Observed = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
