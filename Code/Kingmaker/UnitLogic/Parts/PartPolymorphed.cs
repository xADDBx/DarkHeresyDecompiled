using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartPolymorphed : BaseUnitPart, IHashable, IOwlPackable<PartPolymorphed>
{
	[JsonProperty]
	[CanBeNull]
	[OwlPackInclude]
	public BlueprintPortrait OriginalPortrait;

	[JsonProperty]
	[CanBeNull]
	[OwlPackInclude]
	public PortraitData OriginalPortraitData;

	[JsonProperty]
	[OwlPackInclude]
	public bool RestorePortrait;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartPolymorphed",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("OriginalPortrait", typeof(BlueprintPortrait)),
			new FieldInfo("OriginalPortraitData", typeof(PortraitData)),
			new FieldInfo("RestorePortrait", typeof(bool))
		}
	};

	public Polymorph Component { get; private set; }

	public GameObject ViewReplacement { get; set; }

	[CanBeNull]
	public BlueprintUnit ReplaceBlueprintForInspection => Component?.ReplaceUnitForInspection;

	public void Setup([NotNull] Polymorph component)
	{
		Component = component;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = SimpleBlueprintHasher.GetHash128(OriginalPortrait);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<PortraitData>.GetHash128(OriginalPortraitData);
		result.Append(ref val3);
		result.Append(ref RestorePortrait);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartPolymorphed source = new PartPolymorphed();
		result = Unsafe.As<PartPolymorphed, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartPolymorphed>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "OriginalPortrait", ref OriginalPortrait, state);
		formatter.Field(1, "OriginalPortraitData", ref OriginalPortraitData, state);
		formatter.UnmanagedField(2, "RestorePortrait", ref RestorePortrait, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartPolymorphed>();
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
				OriginalPortrait = formatter.ReadPackable<BlueprintPortrait>(state);
				break;
			case 1:
				OriginalPortraitData = formatter.ReadPackable<PortraitData>(state);
				break;
			case 2:
				RestorePortrait = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
