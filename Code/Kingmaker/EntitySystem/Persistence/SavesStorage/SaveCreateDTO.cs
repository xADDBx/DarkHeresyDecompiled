using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Serialization;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.EntitySystem.Persistence.SavesStorage;

[OwlPackable(OwlPackableMode.Generate)]
public class SaveCreateDTO : IOwlPackable, IOwlPackable<SaveCreateDTO>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SaveCreateDTO",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	[JsonProperty]
	public SaveMetadata Save { get; set; }

	[JsonProperty]
	public List<GameElementData> Elements { get; set; }

	private SaveCreateDTO()
	{
	}

	public static SaveCreateDTO Build([NotNull] SaveInfo save, [CanBeNull] Player player)
	{
		if (player == null)
		{
			try
			{
				string source = save.Saver.ReadJson("player");
				player = SaveSystemJsonSerializer.Serializer.DeserializeObject<Player>(source);
			}
			catch (Exception)
			{
				player = null;
			}
		}
		SaveCreateDTO saveCreateDTO = new SaveCreateDTO
		{
			Save = new SaveMetadata(save, player),
			Elements = new List<GameElementData>()
		};
		if (player != null)
		{
			foreach (KeyValuePair<BlueprintUnlockableFlag, int> unlockedFlag in player.UnlockableFlags.UnlockedFlags)
			{
				saveCreateDTO.Elements.Add(new GameElementData(GameElementType.Flag, unlockedFlag.Key, unlockedFlag.Value.ToString()));
			}
			foreach (Quest quest in Game.Instance.QuestBook.Quests)
			{
				saveCreateDTO.Elements.Add(new GameElementData(GameElementType.Quest, quest.Blueprint, quest.State.ToString()));
				foreach (QuestObjective objective in quest.Objectives)
				{
					saveCreateDTO.Elements.Add(new GameElementData(GameElementType.Objective, objective.Blueprint, objective.State.ToString()));
				}
			}
			foreach (BaseUnitEntity allCrossSceneUnit in player.AllCrossSceneUnits)
			{
				if (allCrossSceneUnit.IsPet)
				{
					continue;
				}
				saveCreateDTO.Elements.Add(new GameElementData(GameElementType.Companion, allCrossSceneUnit.Blueprint, allCrossSceneUnit.LifeState.State.ToString()));
				string value = "Unknown";
				CompanionState companionState = allCrossSceneUnit.GetOptional<UnitPartCompanion>()?.State ?? CompanionState.None;
				if (allCrossSceneUnit.UniqueId == player.MainCharacter.Id)
				{
					value = "Player";
				}
				else
				{
					switch (companionState)
					{
					case CompanionState.InParty:
					case CompanionState.InPartyDetached:
						value = "Party";
						break;
					case CompanionState.ExCompanion:
						value = "Ex";
						break;
					case CompanionState.Remote:
						value = "Remote";
						break;
					}
				}
				saveCreateDTO.Elements.Add(new GameElementData(GameElementType.Companion, allCrossSceneUnit.Blueprint, value));
				if (companionState == CompanionState.InPartyDetached)
				{
					saveCreateDTO.Elements.Add(new GameElementData(GameElementType.Companion, allCrossSceneUnit.Blueprint, "Detached"));
				}
			}
			foreach (int version in save.Versions)
			{
				saveCreateDTO.Elements.Add(new GameElementData(GameElementType.UpgradeVersion, null, version.ToString()));
			}
		}
		return saveCreateDTO;
	}

	private static bool ListHasUnit(IList<UnitReference> list, BaseUnitEntity unit)
	{
		foreach (UnitReference item in list)
		{
			if (item.Id == unit.UniqueId)
			{
				return true;
			}
		}
		return false;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SaveCreateDTO source = new SaveCreateDTO();
		result = Unsafe.As<SaveCreateDTO, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SaveCreateDTO>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SaveCreateDTO>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
