using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Parts;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning;

[OwlPackable(OwlPackableMode.Generate)]
public class PlayerUpgradeAction : IHashable, IOwlPackable, IOwlPackable<PlayerUpgradeAction>
{
	[JsonProperty]
	[OwlPackInclude]
	public PlayerUpgradeActionType Type;

	[JsonProperty]
	[OwlPackInclude]
	public BlueprintScriptableObject Blueprint;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PlayerUpgradeAction",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("Type", typeof(PlayerUpgradeActionType)),
			new FieldInfo("Blueprint", typeof(BlueprintScriptableObject))
		}
	};

	public void Apply()
	{
		switch (Type)
		{
		case PlayerUpgradeActionType.GiveObjective:
			if (Blueprint is BlueprintQuestObjective objecive2)
			{
				GameHelper.Quests.GiveObjective(objecive2);
			}
			break;
		case PlayerUpgradeActionType.CompleteObjective:
			if (Blueprint is BlueprintQuestObjective objecive3)
			{
				GameHelper.Quests.CompleteObjective(objecive3);
			}
			break;
		case PlayerUpgradeActionType.FailObjective:
			if (Blueprint is BlueprintQuestObjective objecive)
			{
				GameHelper.Quests.FailObjective(objecive);
			}
			break;
		case PlayerUpgradeActionType.ResetObjective:
			if (Blueprint is BlueprintQuestObjective bpObjective)
			{
				Game.Instance.QuestBook.ResetObjective(bpObjective);
			}
			break;
		case PlayerUpgradeActionType.GiveItem:
			if (Blueprint is BlueprintItem newBpItem)
			{
				Game.Instance.PartySharedInventory.Collection.Add(newBpItem);
			}
			break;
		case PlayerUpgradeActionType.RemoveItem:
			if (Blueprint is BlueprintItem bpItem)
			{
				Game.Instance.PartySharedInventory.Collection.Remove(bpItem);
			}
			break;
		case PlayerUpgradeActionType.UnrecruitCompanion:
			UnrecruitCompanion(Blueprint as BlueprintUnit);
			break;
		case PlayerUpgradeActionType.AttachAllPartyMembers:
		{
			foreach (BaseUnitEntity allCrossSceneUnit in Game.Instance.Player.AllCrossSceneUnits)
			{
				UnitPartCompanion optional = allCrossSceneUnit.GetOptional<UnitPartCompanion>();
				if (optional != null && optional.State == CompanionState.InPartyDetached)
				{
					Game.Instance.Player.AttachPartyMember(allCrossSceneUnit);
				}
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		case PlayerUpgradeActionType.MakeUnitEssentialForGame:
		case PlayerUpgradeActionType.MakeUnitNotEssentialForGame:
			break;
		}
	}

	[CanBeNull]
	private BaseUnitEntity GetCharacter(BlueprintUnit blueprint)
	{
		if (!blueprint)
		{
			return null;
		}
		return Game.Instance.Player.AllCharacters.Find((BaseUnitEntity u) => u.Blueprint == blueprint);
	}

	private void UnrecruitCompanion(BlueprintUnit blueprintUnit)
	{
		Unrecruit unrecruit = new Unrecruit();
		unrecruit.CompanionBlueprint = blueprintUnit;
		unrecruit.OnUnrecruit = new ActionList();
		unrecruit.Run();
		BaseUnitEntity character = GetCharacter(blueprintUnit);
		if (character != null)
		{
			character.IsInGame = false;
		}
	}

	private static void DeactivateFact(BaseUnitEntity unit, BlueprintUnitFact blueprint)
	{
		unit.Facts.Get(blueprint)?.Deactivate();
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref Type);
		Hash128 val = SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PlayerUpgradeAction source = new PlayerUpgradeAction();
		result = Unsafe.As<PlayerUpgradeAction, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PlayerUpgradeAction>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EnumField(0, "Type", ref Type, state);
		formatter.Field(1, "Blueprint", ref Blueprint, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PlayerUpgradeAction>();
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
				Type = formatter.ReadEnum<PlayerUpgradeActionType>(state);
				break;
			case 1:
				Blueprint = formatter.ReadPackable<BlueprintScriptableObject>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
