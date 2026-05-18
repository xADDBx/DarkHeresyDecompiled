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
[OwlPackOldName("Kingmaker.Framework.DetectiveSystem.DetectiveSystem+AddendumState")]
internal sealed class AddendumState : CaseItemState, IHashable, IOwlPackable<AddendumState>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AddendumState",
		OldNames = new string[1] { "Kingmaker.Framework.DetectiveSystem.DetectiveSystem+AddendumState" },
		Fields = new FieldInfo[4]
		{
			new FieldInfo("Hidden", typeof(bool)),
			new FieldInfo("Source", typeof(BlueprintScriptableObject)),
			new FieldInfo("PlaceOfIssue", typeof(BlueprintArea)),
			new FieldInfo("Found", typeof(bool))
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

	public AddendumState(BlueprintClueAddendum _)
	{
	}

	[UsedImplicitly]
	private AddendumState(OwlPackConstructorParameter _)
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
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		AddendumState source = new AddendumState(default(OwlPackConstructorParameter));
		result = Unsafe.As<AddendumState, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AddendumState>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		bool value = base.Hidden;
		formatter.UnmanagedField(0, "Hidden", ref value, state);
		BlueprintScriptableObject value2 = Source;
		formatter.Field(1, "Source", ref value2, state);
		BlueprintArea value3 = PlaceOfIssue;
		formatter.Field(2, "PlaceOfIssue", ref value3, state);
		bool value4 = Found;
		formatter.UnmanagedField(3, "Found", ref value4, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AddendumState>();
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
			}
		}
		formatter.LeaveObject();
	}
}
