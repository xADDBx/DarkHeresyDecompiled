using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Visual.Blueprints;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Controllers;

[OwlPackable(OwlPackableMode.Generate)]
public class MoraleDataPart : EntityPart, IHashable, IOwlPackable<MoraleDataPart>
{
	[OwlPackInclude]
	public readonly List<MoraleGroup> MoraleGroups = new List<MoraleGroup>();

	[OwlPackInclude]
	public UnitVisualSettings.MusicCombatState ForcedMusicCombatState = UnitVisualSettings.MusicCombatState.None;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "MoraleDataPart",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("MoraleGroups", typeof(List<MoraleGroup>)),
			new FieldInfo("ForcedMusicCombatState", typeof(UnitVisualSettings.MusicCombatState))
		}
	};

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		MoraleDataPart source = new MoraleDataPart();
		result = Unsafe.As<MoraleDataPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<MoraleDataPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		List<MoraleGroup> value = MoraleGroups;
		formatter.Field(0, "MoraleGroups", ref value, state);
		formatter.EnumField(1, "ForcedMusicCombatState", ref ForcedMusicCombatState, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MoraleDataPart>();
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
				Unsafe.AsRef(in MoraleGroups) = formatter.ReadPackable<List<MoraleGroup>>(state);
				break;
			case 1:
				ForcedMusicCombatState = formatter.ReadEnum<UnitVisualSettings.MusicCombatState>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
