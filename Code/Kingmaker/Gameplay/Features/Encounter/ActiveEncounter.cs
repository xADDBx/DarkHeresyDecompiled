using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Code.Gameplay.Controllers;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Encounter.Components;
using Kingmaker.Gameplay.Features.Encounter.Events;
using Kingmaker.Gameplay.Features.Experience;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.EntityBlackboard;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Gameplay.Features.Encounter;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class ActiveEncounter : MechanicEntity<BlueprintEncounter>, IAreaActivationHandler, ISubscriber, IHashable, IOwlPackable<ActiveEncounter>
{
	[OwlPackable(OwlPackableMode.Generate)]
	private class RuntimeEntityBlackboardRecord : IOwlPackable, IOwlPackable<RuntimeEntityBlackboardRecord>
	{
		[OwlPackInclude]
		public EntityRef<MechanicEntity> EntityRef;

		[OwlPackInclude]
		public RuntimeEntityBlackboard Blackboard;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "RuntimeEntityBlackboardRecord",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("EntityRef", typeof(EntityRef<MechanicEntity>)),
				new FieldInfo("Blackboard", typeof(RuntimeEntityBlackboard))
			}
		};

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			RuntimeEntityBlackboardRecord source = new RuntimeEntityBlackboardRecord();
			result = Unsafe.As<RuntimeEntityBlackboardRecord, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<RuntimeEntityBlackboardRecord>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.Field(0, "EntityRef", ref EntityRef, state);
			formatter.Field(1, "Blackboard", ref Blackboard, state);
			formatter.EndObject();
		}

		public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<RuntimeEntityBlackboardRecord>();
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
					EntityRef = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
					break;
				case 1:
					Blackboard = formatter.ReadPackable<RuntimeEntityBlackboard>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public const string ID = "active-encounter-id";

	public new static readonly EntityRef<ActiveEncounter> Ref = new EntityRef<ActiveEncounter>("active-encounter-id");

	[OwlPackInclude]
	private bool _moraleVictoryRejected;

	[OwlPackInclude]
	private RuntimeEntityBlackboard _blackboard = new RuntimeEntityBlackboard();

	[OwlPackInclude]
	private List<RuntimeEntityBlackboardRecord> _participantBlackboards = new List<RuntimeEntityBlackboardRecord>();

	private EncounterCompletionConfirmation? _completionConfirmation;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ActiveEncounter",
		OldNames = null,
		Fields = new FieldInfo[19]
		{
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_IsInGame", typeof(bool)),
			new FieldInfo("m_Position", typeof(Vector3)),
			new FieldInfo("m_Orientation", typeof(float)),
			new FieldInfo("m_InitialPosition", typeof(Vector3?)),
			new FieldInfo("m_InitialOrientation", typeof(float?)),
			new FieldInfo("Facts", typeof(EntityFactsManager)),
			new FieldInfo("Parts", typeof(EntityPartsManager)),
			new FieldInfo("m_IsRevealed", typeof(bool)),
			new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?)),
			new FieldInfo("m_Initiative", typeof(Initiative)),
			new FieldInfo("m_OriginalBlueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("m_Blueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("MainFact", typeof(MechanicEntityFact)),
			new FieldInfo("IsPartyAmbushed", typeof(bool)),
			new FieldInfo("IsCompleted", typeof(bool)),
			new FieldInfo("_moraleVictoryRejected", typeof(bool)),
			new FieldInfo("_blackboard", typeof(RuntimeEntityBlackboard)),
			new FieldInfo("_participantBlackboards", typeof(List<RuntimeEntityBlackboardRecord>))
		}
	};

	[OwlPackInclude]
	public bool IsPartyAmbushed { get; private set; }

	[OwlPackInclude]
	public bool IsCompleted { get; private set; }

	public static ActiveEncounter? Current => Ref.Entity;

	public IEnumerable<BaseUnitEntity> Participants => Game.Instance.EntityPools.AllBaseUnits.Where((BaseUnitEntity i) => i.Encounter?.Blueprint == base.Blueprint);

	public override bool NeedsView => false;

	public bool IsDefault => base.Blueprint.IsDefault;

	public RuntimeEntityBlackboard Blackboard => _blackboard;

	public bool IsWaitingForMoraleVictoryConfirmation
	{
		get
		{
			EncounterCompletionConfirmation completionConfirmation = _completionConfirmation;
			if (completionConfirmation is EncounterCompletionByMoraleConfirmation)
			{
				return !completionConfirmation.IsRejected;
			}
			return false;
		}
	}

	public static ActiveEncounter Start(BlueprintEncounter blueprint, bool isPartyAmbushed = false)
	{
		ActiveEncounter current = Current;
		if (current != null && !current.IsDefault)
		{
			throw new InvalidOperationException("Can't start encounter " + blueprint.name + " because " + Current.Blueprint.name + " is already active.");
		}
		if (Game.Instance.Player.CompletedEncounters.Contains(blueprint))
		{
			throw new InvalidOperationException("Can't start encounter " + blueprint.name + " because it was already completed.");
		}
		List<BaseUnitEntity> value;
		using (CollectionPool<List<BaseUnitEntity>, BaseUnitEntity>.Get(out value))
		{
			current = Current;
			if (current != null && current.IsDefault)
			{
				value.AddRange(Current.Participants);
			}
			Current?.Dispose();
			ActiveEncounter activeEncounter = Entity.Initialize(new ActiveEncounter(blueprint, isPartyAmbushed));
			Game.Instance.Player.CrossSceneState.AddEntityData(activeEncounter);
			activeEncounter.Setup(value);
			return activeEncounter;
		}
	}

	private ActiveEncounter(BlueprintEncounter blueprint, bool isPartyAmbushed)
		: base("active-encounter-id", isInGame: true, blueprint)
	{
		IsPartyAmbushed = isPartyAmbushed;
	}

	private ActiveEncounter(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	protected override IEntityView? CreateViewForData()
	{
		return null;
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		GetOrCreate<PartEncounterMoraleVictoryWatcher>();
		GetOrCreate<PartEncounterMetrics>();
		if (base.Blueprint.HasComponent<EncounterObjectivesComponent>())
		{
			GetOrCreate<PartEncounterObjectives>();
		}
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		SetupBlackboard();
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		SendStartMetrics(isLoaded: true);
	}

	private void Setup(IEnumerable<BaseUnitEntity> previousEncounterParticipants)
	{
		SetupParticipants();
		foreach (BaseUnitEntity previousEncounterParticipant in previousEncounterParticipants)
		{
			AddParticipant(previousEncounterParticipant);
		}
		SendStartMetrics(isLoaded: false);
		base.EventBus.RaiseEvent((IMechanicEntity)this, (Action<IStartEncounterHandler>)delegate(IStartEncounterHandler h)
		{
			h.HandleStartEncounter();
		}, isCheckRuntime: true);
		base.EventBus.RaiseEvent(delegate(IPartyCombatHandler h)
		{
			h.HandlePartyCombatStateChanged(inCombat: true);
		});
	}

	private void SendStartMetrics(bool isLoaded)
	{
		List<BaseUnitEntity> list = Participants.Where((BaseUnitEntity p) => p.IsPlayerFaction).ToList();
		PartEncounterMetrics optional = GetOptional<PartEncounterMetrics>();
		Metrics.EncounterStart.IsLoaded(isLoaded).PartyCount(list.Count).EnemiesCount(Participants.Count((BaseUnitEntity p) => p.IsPlayerEnemy))
			.ExperienceLevel(Game.Instance.Player.MainCharacterEntity.GetProgressionOptional()?.ExperienceLevel ?? (-1))
			.PartyHealth(list.Select((BaseUnitEntity p) => p.Health.HitPointsLeft).Sum())
			.PartyArmour(list.Select((BaseUnitEntity p) => p.Armor.DurabilityLeft).Sum())
			.DamageToParty(optional?.DamageToParty ?? 0)
			.DamageToEnemies(optional?.DamageToEnemies ?? 0)
			.Difficulty(MetricsUtils.GameDifficultyToString(SettingsRoot.Difficulty.GameDifficulty.GetValue()))
			.CombatLog(Game.Instance.Player.UISettings.LogIsPinned)
			.Send();
		if (isLoaded)
		{
			return;
		}
		List<string> value;
		using (CollectionPool<List<string>, string>.Get(out value))
		{
			List<string> value2;
			using (CollectionPool<List<string>, string>.Get(out value2))
			{
				foreach (BaseUnitEntity item in list)
				{
					PartAbilityModifiers optional2 = item.GetOptional<PartAbilityModifiers>();
					if (optional2 != null)
					{
						foreach (Ability ability in item.Abilities)
						{
							BlueprintAbilityModifier manuallyAddedModifier = optional2.GetManuallyAddedModifier(ability);
							if (manuallyAddedModifier != null)
							{
								value.Add(manuallyAddedModifier.AssetGuid);
								value2.Add(ability.Blueprint.AssetGuid);
							}
						}
					}
					Metrics.EncounterCompanionStart.Id(item.Blueprint.AssetGuid).Abilities(item.Abilities.Enumerable.Select((Ability a) => a.Blueprint.AssetGuid)).Equipment(from s in item.Body.AllSlots
						where s.HasItem
						select s.Item.Blueprint.AssetGuid)
						.ModifiersTargetsId(value)
						.ModifiersTargetsAbility(value2)
						.Send();
					value.Clear();
					value2.Clear();
				}
			}
		}
	}

	private void SendFinishMetrics(EncounterCompletionType completionType)
	{
		List<BaseUnitEntity> list = Participants.Where((BaseUnitEntity p) => p.IsPlayerFaction).ToList();
		Metrics.EncounterFinish.PartyHealth(list.Select((BaseUnitEntity p) => p.Health.HitPointsLeft).Sum()).PartyArmour(list.Select((BaseUnitEntity p) => p.Armor.DurabilityLeft).Sum()).Reason(completionType)
			.PowerBalance(Game.Instance.Controllers.MoraleController.GetPlayerPowerBalanceRatio())
			.Difficulty(MetricsUtils.GameDifficultyToString(SettingsRoot.Difficulty.GameDifficulty.GetValue()))
			.Send();
		PartEncounterMetrics optional = GetOptional<PartEncounterMetrics>();
		if (optional == null)
		{
			return;
		}
		List<string> value;
		using (CollectionPool<List<string>, string>.Get(out value))
		{
			List<string> value2;
			using (CollectionPool<List<string>, string>.Get(out value2))
			{
				foreach (BaseUnitEntity item in list)
				{
					foreach (Ability ability in item.Abilities)
					{
						string assetGuid = ability.Blueprint.AssetGuid;
						value.Add(assetGuid);
						value2.Add(optional.GetAbilityCastCount(item.Blueprint.AssetGuid, assetGuid).ToString());
					}
					Metrics.EncounterCompanionFinish.Id(item.Blueprint.AssetGuid).AbilitiesUsagesId(value).AbilitiesUsagesCount(value2)
						.Send();
					value.Clear();
					value2.Clear();
				}
			}
		}
	}

	private void SetupBlackboard()
	{
		if (base.Blueprint.TryGetComponent<EntityBlackboardComponent>(out var component))
		{
			_blackboard = new RuntimeEntityBlackboard(component);
		}
	}

	private void SetupParticipants()
	{
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.EntityPools.AllBaseUnits)
		{
			if (allBaseUnit.IsInPlayerParty || allBaseUnit.Encounter?.Blueprint == base.Blueprint)
			{
				AddParticipant(allBaseUnit);
			}
		}
	}

	public void AddParticipant(BaseUnitEntity participant)
	{
		participant.GetOrCreate<PartEncounter>().Join(base.Blueprint);
	}

	private bool IsAllEnemiesDeadVictoryConditionMet()
	{
		if (!base.Blueprint.AllowVictoryWhenAllEnemiesDead)
		{
			return false;
		}
		foreach (BaseUnitEntity participant in Participants)
		{
			if (participant != null && !participant.IsDead && participant.IsPlayerEnemy && !participant.SpawnFromPsychicPhenomena && participant.IsInCombat)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsMoraleVictoryConditionMet()
	{
		if (!base.Blueprint.AllowVictoryByMorale || _moraleVictoryRejected)
		{
			return false;
		}
		return GetRequired<PartEncounterMoraleVictoryWatcher>().IsMoraleVictoryAllowed;
	}

	public bool IsValidByDefaultMoraleConditions()
	{
		return Game.Instance.Controllers.MoraleController.MoraleGroups.AllItems((MoraleGroup i) => !i.IsPlayerEnemy || (i.PowerBalanceState == PowerBalanceState.Shattered && i.GroupCanSurrender));
	}

	private bool IsCustomVictoryConditionMet()
	{
		if (base.Blueprint.AllowVictoryByCustomCondition)
		{
			return base.Blueprint.CustomVictoryCondition.Check();
		}
		return false;
	}

	private void Complete(EncounterCompletionType completionType)
	{
		IsCompleted = true;
		if (!IsDefault)
		{
			Game.Instance.Player.CompletedEncounters.Add(base.Blueprint);
		}
		Kingmaker.Gameplay.Features.Experience.Experience.GainForEncounter(base.Blueprint);
		base.Blueprint.OnVictory.Run();
		switch (completionType)
		{
		case EncounterCompletionType.AllEnemiesDead:
			base.Blueprint.OnVictoryWhenAllEnemiesDead.Run();
			break;
		case EncounterCompletionType.Morale:
			base.Blueprint.OnVictoryByMorale.Run();
			break;
		case EncounterCompletionType.Custom:
			base.Blueprint.OnVictoryByCustomCondition.Run();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		foreach (BaseUnitEntity allCharacter in Game.Instance.Player.AllCharacters)
		{
			allCharacter.Buffs.OnCombatEnded();
			allCharacter.Abilities.OnCombatEnd();
		}
		SendFinishMetrics(completionType);
		GetOptional<PartEncounterMetrics>()?.Reset();
		Game.Instance.Controllers.TurnController.ExitTb();
		CleanupParticipantsOnComplete();
		Facts.Dispose();
		Dispose();
		base.EventBus.RaiseEvent(delegate(ICombatEndHandler h)
		{
			h.HandleCombatEnd(completionType);
		});
		base.EventBus.RaiseEvent(delegate(IPartyCombatHandler h)
		{
			h.HandlePartyCombatStateChanged(inCombat: false);
		});
	}

	private void CleanupParticipantsOnComplete()
	{
		List<BaseUnitEntity> list;
		using (Participants.ToPooledList(out list))
		{
			foreach (BaseUnitEntity item in list)
			{
				if ((item.IsInCombat && item.IsPlayerEnemy) || item.SpawnFromPsychicPhenomena || item.Buffs.Contains((BlueprintBuff?)ConfigRoot.Instance.MoraleRoot.BetrayalBuff))
				{
					item.LifeState.ManualDeath();
					UnitLifeController.ForceTickOnUnit(item);
				}
				item.Remove<PartEncounter>();
				if (item.IsInCombat)
				{
					item.CombatState.LeaveCombat();
				}
			}
		}
	}

	private bool IsTurnControllerReady()
	{
		TurnController turnController = Game.Instance.Controllers.TurnController;
		if (turnController != null && turnController.InCombat && !turnController.IsManualCombatTurn)
		{
			return !turnController.IsPreparationTurn;
		}
		return false;
	}

	public bool TryComplete()
	{
		if (!IsTurnControllerReady())
		{
			return false;
		}
		if (_completionConfirmation == null)
		{
			if (IsAllEnemiesDeadVictoryConditionMet())
			{
				_completionConfirmation = new EncounterCompletionWithDelayConfirmation(EncounterCompletionType.AllEnemiesDead);
			}
			else if (IsCustomVictoryConditionMet())
			{
				_completionConfirmation = new EncounterCompletionWithDelayConfirmation(EncounterCompletionType.Custom);
			}
			else if (IsMoraleVictoryConditionMet())
			{
				_completionConfirmation = new EncounterCompletionByMoraleConfirmation(EncounterCompletionType.Morale);
			}
		}
		EncounterCompletionConfirmation completionConfirmation = _completionConfirmation;
		if (completionConfirmation != null)
		{
			completionConfirmation.Update();
			if (completionConfirmation.IsConfirmed)
			{
				Complete(completionConfirmation.Type);
				return true;
			}
			if (completionConfirmation.IsRejected)
			{
				if (!(completionConfirmation is EncounterCompletionByMoraleConfirmation))
				{
					throw new InvalidOperationException("Encounter victory confirmation rejected.");
				}
				_moraleVictoryRejected = true;
				_completionConfirmation.Dispose();
				_completionConfirmation = null;
			}
		}
		return false;
	}

	public bool TryCompleteImmediately()
	{
		if (!IsTurnControllerReady())
		{
			return false;
		}
		if (IsAllEnemiesDeadVictoryConditionMet())
		{
			Complete(EncounterCompletionType.AllEnemiesDead);
			return true;
		}
		if (IsCustomVictoryConditionMet() || Game.Instance.Controllers.TurnController.IsManualCombatTurn)
		{
			Complete(EncounterCompletionType.Custom);
			return true;
		}
		return false;
	}

	public void SetVictoryByMoraleConfirmed(bool confirmed)
	{
		((_completionConfirmation as EncounterCompletionByMoraleConfirmation) ?? throw new Exception("Encounter completion confirmation was not set by morale")).Callback(confirmed);
	}

	public void StoreParticipantRuntimeState(EntityRef<MechanicEntity> participant, IRuntimeEntityBlackboard runtimeState)
	{
		RuntimeEntityBlackboardRecord runtimeEntityBlackboardRecord = _participantBlackboards.FindOrDefault<RuntimeEntityBlackboardRecord>((RuntimeEntityBlackboardRecord r) => r.EntityRef == (MechanicEntity)participant);
		if (runtimeEntityBlackboardRecord == null)
		{
			runtimeEntityBlackboardRecord = new RuntimeEntityBlackboardRecord
			{
				EntityRef = participant
			};
			_participantBlackboards.Add(runtimeEntityBlackboardRecord);
		}
		runtimeEntityBlackboardRecord.Blackboard = new RuntimeEntityBlackboard(runtimeState);
	}

	public IRuntimeEntityBlackboard GetParticipantRuntimeState(EntityRef<MechanicEntity> participant)
	{
		return _participantBlackboards.FindOrDefault<RuntimeEntityBlackboardRecord>((RuntimeEntityBlackboardRecord r) => r.EntityRef == (MechanicEntity)participant)?.Blackboard;
	}

	public override string ToString()
	{
		return $"ActiveEncounter[{base.Blueprint}]";
	}

	void IAreaActivationHandler.OnAreaActivated()
	{
		base.EventBus.RaiseEvent(delegate(IPartyCombatHandler h)
		{
			h.HandlePartyCombatStateChanged(inCombat: true);
		});
	}

	protected override void OnDispose()
	{
		_completionConfirmation?.Dispose();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ActiveEncounter source = new ActiveEncounter(default(OwlPackConstructorParameter));
		result = Unsafe.As<ActiveEncounter, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ActiveEncounter>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.UniqueId;
		formatter.StringField(0, "UniqueId", ref value, state);
		formatter.UnmanagedField(1, "m_IsInGame", ref m_IsInGame, state);
		formatter.Field(2, "m_Position", ref m_Position, state);
		formatter.UnmanagedField(3, "m_Orientation", ref m_Orientation, state);
		formatter.NullableField(4, "m_InitialPosition", ref m_InitialPosition, state);
		formatter.UnmanagedNullableField(5, "m_InitialOrientation", ref m_InitialOrientation, state);
		formatter.Field(6, "Facts", ref Facts, state);
		formatter.Field(7, "Parts", ref Parts, state);
		formatter.UnmanagedField(8, "m_IsRevealed", ref m_IsRevealed, state);
		formatter.EnumNullableField(9, "m_ViewHandlingOnDisposePolicyOverride", ref m_ViewHandlingOnDisposePolicyOverride, state);
		formatter.Field(10, "m_Initiative", ref m_Initiative, state);
		formatter.Field(11, "m_OriginalBlueprint", ref m_OriginalBlueprint, state);
		formatter.Field(12, "m_Blueprint", ref m_Blueprint, state);
		MechanicEntityFact value2 = base.MainFact;
		formatter.Field(13, "MainFact", ref value2, state);
		bool value3 = IsPartyAmbushed;
		formatter.UnmanagedField(14, "IsPartyAmbushed", ref value3, state);
		bool value4 = IsCompleted;
		formatter.UnmanagedField(15, "IsCompleted", ref value4, state);
		formatter.UnmanagedField(16, "_moraleVictoryRejected", ref _moraleVictoryRejected, state);
		formatter.Field(17, "_blackboard", ref _blackboard, state);
		formatter.Field(18, "_participantBlackboards", ref _participantBlackboards, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ActiveEncounter>();
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
				base.UniqueId = formatter.ReadString(state);
				break;
			case 1:
				m_IsInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_Position = formatter.ReadPackable<Vector3>(state);
				break;
			case 3:
				m_Orientation = formatter.ReadUnmanaged<float>(state);
				break;
			case 4:
				m_InitialPosition = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 5:
				m_InitialOrientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 6:
				Facts = formatter.ReadPackable<EntityFactsManager>(state);
				break;
			case 7:
				Parts = formatter.ReadPackable<EntityPartsManager>(state);
				break;
			case 8:
				m_IsRevealed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				m_ViewHandlingOnDisposePolicyOverride = formatter.ReadNullableEnum<ViewHandlingOnDisposePolicyType>(state);
				break;
			case 10:
				m_Initiative = formatter.ReadPackable<Initiative>(state);
				break;
			case 11:
				m_OriginalBlueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 12:
				m_Blueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 13:
				base.MainFact = formatter.ReadPackable<MechanicEntityFact>(state);
				break;
			case 14:
				IsPartyAmbushed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 15:
				IsCompleted = formatter.ReadUnmanaged<bool>(state);
				break;
			case 16:
				_moraleVictoryRejected = formatter.ReadUnmanaged<bool>(state);
				break;
			case 17:
				_blackboard = formatter.ReadPackable<RuntimeEntityBlackboard>(state);
				break;
			case 18:
				_participantBlackboards = formatter.ReadPackable<List<RuntimeEntityBlackboardRecord>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
