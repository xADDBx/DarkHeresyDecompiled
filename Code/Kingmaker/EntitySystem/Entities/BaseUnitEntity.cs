using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Designers.Mechanics.Facts;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Optimization;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.Gameplay.Features.UnitStats;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking.Serialization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Kingmaker.Settings;
using Kingmaker.Sound;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.UnitLogic.Progression.Prerequisites;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.CountingGuard;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Kingmaker.Visual.HitSystem;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using Pathfinding;
using R3;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class BaseUnitEntity : AbstractUnitEntity, PartUnitCombatState.IOwner, IEntityPartOwner<PartUnitCombatState>, IEntityPartOwner, PartFaction.IOwner, IEntityPartOwner<PartFaction>, PartCombatGroup.IOwner, IEntityPartOwner<PartCombatGroup>, PartVision.IOwner, IEntityPartOwner<PartVision>, PartUnitStealth.IOwner, IEntityPartOwner<PartUnitStealth>, PartUnitProgression.IOwner, IEntityPartOwner<PartUnitProgression>, PartStatsAttributes.IOwner, IEntityPartOwner<PartStatsAttributes>, PartStatsSkills.IOwner, IEntityPartOwner<PartStatsSkills>, PartUnitProficiency.IOwner, IEntityPartOwner<PartUnitProficiency>, PartAbilityResourceCollection.IOwner, IEntityPartOwner<PartAbilityResourceCollection>, PartInventory.IOwner, IEntityPartOwner<PartInventory>, PartUnitBody.IOwner, IEntityPartOwner<PartUnitBody>, PartUnitDescription.IOwner, IEntityPartOwner<PartUnitDescription>, PartAbilityCooldowns.IOwner, IEntityPartOwner<PartAbilityCooldowns>, PartActionBar.IOwner, PartUnitAlignment.IOwner, IEntityPartOwner<PartUnitAlignment>, ILootable, IBaseUnitEntity, IAbstractUnitEntity, IMechanicEntity, IEntity, IDisposable, ICombatParticipant, IHashable, IOwlPackable<BaseUnitEntity>
{
	public sealed class OverrideUnitCR : SimpleContextData<int, OverrideUnitCR>
	{
	}

	public new interface IUnitAsleepHandler<TTag> : IUnitAsleepHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IUnitAsleepHandler, TTag>
	{
	}

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	[OwlPackInclude]
	protected List<BlueprintUnitUpgrader> m_AppliedUpgraders;

	[JsonProperty(PropertyName = "IsExtra")]
	[GameStateIgnore]
	[OwlPackInclude]
	protected bool m_IsExtra;

	private MechanicsContext m_CachedContext;

	private bool m_IsCloneOfManeCharacter;

	public readonly CountableFlag PreventDirectControl = new CountableFlag();

	private bool m_LootViewed;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public bool IsSelected { get; set; } = true;


	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public bool IsLink { get; set; } = true;


	[JsonProperty]
	[OwlPackInclude]
	public float TimeToNextRoundTick { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public int CachedPerceptionRoll { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public TimeSpan? LastRestTime { get; set; }

	public override bool IsExtra => m_IsExtra;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public bool SpawnFromPsychicPhenomena { get; protected set; }

	public ReactiveCommand<Unit> UpdateCommand { get; } = new ReactiveCommand<Unit>();


	public EntityRef<BaseUnitEntity> CopyOf { get; set; }

	public AbilityCollection Abilities { get; private set; }

	public ToggleAbilityCollection ToggleAbilities { get; private set; }

	public override bool LootViewed => m_LootViewed;

	public AkStateReference MusicBossFightType { get; set; }

	public PortraitData Portrait => UISettings.Portrait;

	public override bool IsInLockControlCutscene => CutsceneControlledUnit.GetControllingPlayer(this)?.HasActiveLockControl ?? false;

	public override bool AddToGrid => true;

	public new UnitEntityView View => (UnitEntityView)base.View;

	public bool AiMovementForbidden
	{
		get
		{
			if (!IsDirectlyControllable || !base.HoldState)
			{
				return !base.CanMove;
			}
			return true;
		}
	}

	public override bool IsDeadAndHasLoot
	{
		get
		{
			if (base.LifeState.IsFinallyDead)
			{
				return Inventory.HasLoot;
			}
			return false;
		}
	}

	public bool IsMainCharacter
	{
		get
		{
			if (GetOptional<UnitPartMainCharacter>() == null)
			{
				if (CopyOf.Entity != null)
				{
					return CopyOf.Entity.IsMainCharacter;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsCloneOfMainCharacter
	{
		get
		{
			if (m_IsCloneOfManeCharacter)
			{
				return true;
			}
			return Game.Instance.Player.MainCharacterEntity?.Blueprint == base.Blueprint;
		}
	}

	public override bool IsDirectlyControllable
	{
		get
		{
			if ((bool)PreventDirectControl)
			{
				return false;
			}
			AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
			if (loadedAreaState != null && loadedAreaState.Settings.CapitalPartyMode)
			{
				if (this != Game.Instance.Player.MainCharacterEntity)
				{
					return Master == Game.Instance.Player.MainCharacterEntity;
				}
				return true;
			}
			if (!Faction.IsDirectlyControllable || base.LifeState.IsFinallyDead || IsDetached)
			{
				return false;
			}
			if (GetOptional<UnitPartSummonedMonster>() != null)
			{
				return Faction.IsDirectlyControllable;
			}
			UnitPartCompanion unitPartCompanion = Master?.GetOptional<UnitPartCompanion>() ?? GetOptional<UnitPartCompanion>();
			if (unitPartCompanion != null && unitPartCompanion.State != CompanionState.ExCompanion)
			{
				return unitPartCompanion.State != CompanionState.Remote;
			}
			return false;
		}
	}

	public override bool AreHandsBusyWithAnimation
	{
		get
		{
			CountingGuard countingGuard = ObjectExtensions.Or(View, null)?.HandsEquipment?.AreHandsBusyWithAnimation;
			if (countingGuard == null)
			{
				return false;
			}
			return countingGuard;
		}
	}

	public BloodType BloodType => base.Blueprint.VisualSettings.BloodType;

	public bool IsDetached
	{
		get
		{
			BaseUnitEntity master = Master;
			if (master?.View != null)
			{
				return master.IsDetached;
			}
			UnitPartCompanion optional = GetOptional<UnitPartCompanion>();
			if (optional == null)
			{
				return false;
			}
			return optional.State == CompanionState.InPartyDetached;
		}
	}

	public override float Corpulence
	{
		get
		{
			if (!View)
			{
				return base.Corpulence;
			}
			return View.Corpulence;
		}
	}

	public bool SilentCaster => GetOptional<PartPolymorphed>()?.Component?.SilentCaster ?? base.Blueprint.VisualSettings.SilentCaster;

	[CanBeNull]
	public BaseUnitEntity Master => null;

	public bool IsPet => false;

	public override ViewHandlingOnDisposePolicyType DefaultViewHandlingOnDisposePolicy => ViewHandlingOnDisposePolicyType.Destroy;

	public override bool IsSuppressible => true;

	public override bool IsAffectedByFogOfWar => true;

	public override bool AlwaysRevealedInFogOfWar => IsDirectlyControllable;

	public override Type RequiredBlueprintType => typeof(BlueprintUnitFact);

	[NotNull]
	public MechanicsContext Context
	{
		get
		{
			MechanicsContext mechanicsContext = m_CachedContext;
			if (mechanicsContext == null)
			{
				MechanicsContext obj = base.MainFact.MaybeContext ?? MechanicsContext.Claim(base.OriginalBlueprint, this);
				MechanicsContext mechanicsContext2 = obj;
				m_CachedContext = obj;
				mechanicsContext = mechanicsContext2;
			}
			return mechanicsContext;
		}
	}

	public override Size OriginalSize
	{
		get
		{
			PartUnitProgression progression = Progression;
			return ((progression == null) ? null : SimpleBlueprintExtendAsObject.Or(progression.Race, null)?.Size) ?? base.Blueprint.Size;
		}
	}

	public bool HasUMDSkill => Skills.SkillLoreXenos.BaseValue > 0;

	public bool IsEssentialForGame
	{
		get
		{
			if (!GetOptional<UnitPartMainCharacter>())
			{
				return GetOptional<UnitPartEssential>();
			}
			return true;
		}
	}

	public BlueprintUnit BlueprintForInspection => GetOptional<PartPolymorphed>()?.ReplaceBlueprintForInspection ?? base.Blueprint;

	public bool IsInvisible => GetOptional<PartUnitInvisible>();

	public override bool IsCheater
	{
		get
		{
			if (!base.IsCheater)
			{
				return base.Blueprint.IsCheater;
			}
			return true;
		}
	}

	public override UnitMovementAgentBase MaybeMovementAgent => ObjectExtensions.Or(View, null)?.MovementAgent;

	public override bool CanBeAttackedDirectly => true;

	public override bool IsPreviewUnit => GetOptional<PartPreviewUnit>() != null;

	public bool HasAssassinCareer => Facts.Contains(ConfigRoot.Instance.SystemMechanics.AssassinCareerPath);

	public override bool CanDodge
	{
		get
		{
			if (!base.IsHelpless)
			{
				return !base.Features.CantAct;
			}
			return false;
		}
	}

	public override bool CanDodgeWithMove
	{
		get
		{
			if (CanDodge && !base.Features.CantMove && !base.IsProne)
			{
				return !base.Features.CantJumpAside;
			}
			return false;
		}
	}

	bool ICombatParticipant.Active
	{
		get
		{
			if (!base.Features.ControlledByDirector && !IsExtra)
			{
				return !base.Features.RemoveFromInitiative;
			}
			return false;
		}
	}

	public override IEnumerable<BlueprintBodyPart> BodyParts
	{
		get
		{
			PartAdditionalBodyParts optional = GetOptional<PartAdditionalBodyParts>();
			if (optional == null)
			{
				return base.BodyParts;
			}
			return base.BodyParts.Concat(optional.List);
		}
	}

	public int CR
	{
		get
		{
			int? overrideCR = base.Blueprint.OverrideCR;
			if (!overrideCR.HasValue)
			{
				if (SimpleContextData<int, OverrideUnitCR>.Current == 0)
				{
					return Game.Instance.CurrentlyLoadedArea?.GetCR() ?? 0;
				}
				return SimpleContextData<int, OverrideUnitCR>.Current;
			}
			return overrideCR.GetValueOrDefault();
		}
	}

	public abstract PartUnitCombatState CombatState { get; }

	public abstract PartFaction Faction { get; }

	public abstract PartCombatGroup CombatGroup { get; }

	public abstract PartVision Vision { get; }

	[Obsolete]
	public abstract PartUnitStealth Stealth { get; }

	public abstract PartUnitProgression Progression { get; }

	public abstract PartStatsAttributes Attributes { get; }

	public abstract PartStatsSkills Skills { get; }

	public abstract PartUnitProficiency Proficiencies { get; }

	public abstract PartAbilityResourceCollection AbilityResources { get; }

	public abstract PartInventory Inventory { get; }

	public abstract PartUnitDescription Description { get; }

	public abstract PartUnitBody Body { get; }

	public PartUnitAlignment Alignment => GetOrCreate<PartUnitAlignment>();

	public PartAbilityCooldowns AbilityCooldowns => GetRequired<PartAbilityCooldowns>();

	public PartUnitUISettings UISettings => GetOrCreate<PartUnitUISettings>();

	public PartActionBar ActionBar => GetOrCreate<PartActionBar>();

	[CanBeNull]
	public UnitPartEncumbrance EncumbranceData => GetOptional<UnitPartEncumbrance>();

	[CanBeNull]
	public PartEncounter Encounter => GetOptional<PartEncounter>();

	string ILootable.Name => base.CharacterName;

	string ILootable.Description => null;

	BaseUnitEntity ILootable.OwnerEntity => this;

	public ItemsCollection Items => Inventory.Collection;

	public List<BlueprintCargoReference> Cargo => null;

	public Func<ItemEntity, bool> CanInsertItem => null;

	public bool MeetsPrerequisite(PrerequisiteStat stat)
	{
		return GetStatOptional(stat.Stat)?.PermanentValue >= stat.MinValue;
	}

	public void MarkLootViewed()
	{
		m_LootViewed = true;
	}

	public LosCalculations.CoverType GetCoverType()
	{
		return LosCalculations.GetCoverType((GridNode)base.CurrentNode.node);
	}

	protected BaseUnitEntity(string uniqueId, bool isInGame, BlueprintUnit blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	protected BaseUnitEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected BaseUnitEntity()
	{
	}

	public void UpdateVisible()
	{
		View.SetVisible(!IsInvisible || CombatGroup.IsPlayerParty, force: true);
	}

	public override float GetWarhammerMovementApPerCellThreateningArea()
	{
		return PartWarhammerMovementApPerCellThreateningArea.GetThreateningArea(this);
	}

	protected override MechanicEntityFact CreateMainFact(BlueprintMechanicEntityFact blueprint)
	{
		return new UnitFact((BlueprintUnit)blueprint, null);
	}

	public override StatBaseValue GetStatBaseValue(StatType type)
	{
		return UnitBaseStats.Get(base.OriginalBlueprint, SettingsRoot.Difficulty.NPCDifficulty, CR)[type];
	}

	protected override void OnPrepareOrPrePostLoad()
	{
		base.OnPrepareOrPrePostLoad();
		Abilities = Facts.EnsureFactProcessor<AbilityCollection>();
		Abilities.SetSubscribedOnEventBus(base.IsInGame);
		ToggleAbilities = Facts.EnsureFactProcessor<ToggleAbilityCollection>();
		ToggleAbilities.SetSubscribedOnEventBus(base.IsInGame);
	}

	protected override void OnApplyPostLoadFixes()
	{
		base.OnApplyPostLoadFixes();
		if (Inventory.Owner == this)
		{
			Inventory?.ApplyPostLoadFixes();
		}
		UnitUpgraderHelper.ApplyUpgraders(this, base.OriginalBlueprint, fromPlaceholder: false, ref m_AppliedUpgraders);
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		GetOrCreate<PartStatsContainer>();
		GetOrCreate<PartAbilityCooldowns>();
		GetOrCreate<EntityBoundsPart>();
		GetOrCreate<PartProvidesCover>();
		if ((bool)ContextData<UnitHelper.PreviewUnit>.Current)
		{
			GetOrCreate<PartPreviewUnit>();
		}
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		GetOrCreate<LevelUpPlanUnitHolder>();
		if (!Faction.IsPlayer)
		{
			AddFact(ConfigRoot.Instance.SystemMechanics.CommonMobFact);
			foreach (BlueprintUnitFact difficultyFact in ConfigRoot.Instance.DifficultyRoot.GetDifficultyFacts(base.OriginalBlueprint.DifficultyType))
			{
				AddFact(difficultyFact);
			}
		}
		using (ProfileScope.New("Add Facts from army"))
		{
			BlueprintArmyType army = base.OriginalBlueprint.Army;
			if (army != null)
			{
				foreach (BlueprintFeature feature in army.Features)
				{
					if (feature != null)
					{
						DoNotAddFeatureFromArmy component = base.Blueprint.GetComponent<DoNotAddFeatureFromArmy>();
						if (component == null || !component.Features.Contains(feature))
						{
							AddFact(feature);
						}
					}
				}
			}
		}
		using (ProfileScope.New("Add Facts"))
		{
			foreach (BlueprintUnitFact item in base.OriginalBlueprint.AddFacts.EmptyIfNull().NotNull())
			{
				AddFact(item).AddSource(base.OriginalBlueprint);
			}
			foreach (InitialAlignmentShift initialAlignmentShift in base.OriginalBlueprint.InitialAlignmentShifts)
			{
				Alignment.SetMark(initialAlignmentShift.Axis, initialAlignmentShift.Mark, base.OriginalBlueprint);
			}
			foreach (BlueprintUnitFact item2 in base.OriginalBlueprint.GetFactsFromTemplate().EmptyIfNull().NotNull())
			{
				AddFact(item2).AddSource(base.OriginalBlueprint);
			}
		}
		if (!ContextData<UnitHelper.ChargenUnit>.Current)
		{
			using (ProfileScope.New("Starting Inventory"))
			{
				AddStartingInventory();
			}
		}
		Remove<LevelUpPlanUnitHolder>();
		UnitUpgraderHelper.SetAllUpgradersApplied(base.OriginalBlueprint, fromPlaceholder: false, ref m_AppliedUpgraders);
	}

	public void AddStartingInventory()
	{
		if ((bool)ContextData<UnitHelper.DoNotCreateItems>.Current)
		{
			return;
		}
		using (ProfileScope.New("Add Item"))
		{
			using (ContextData<ItemsCollection.SuppressEvents>.Request())
			{
				foreach (BlueprintItem item in base.OriginalBlueprint.StartingInventory.NotNull())
				{
					Inventory.Add(item);
				}
			}
		}
	}

	protected override void OnIsInGameChanged()
	{
		base.OnIsInGameChanged();
		if ((bool)Parts.GetOptional<UnitPartCompanion>())
		{
			Game.Instance.Player.InvalidateCharacterLists();
		}
		Abilities.SetSubscribedOnEventBus(base.IsInGame);
		ToggleAbilities.SetSubscribedOnEventBus(base.IsInGame);
	}

	protected override void OnDestroy()
	{
		if (Inventory.IsLootDroppedAsEntity)
		{
			Inventory.TransferInventoryToDroppedLoot();
		}
		base.OnDestroy();
	}

	protected override void OnDispose()
	{
		if (IsInCombat)
		{
			CombatState.LeaveCombat();
		}
		base.OnDispose();
	}

	public UnitEntityView CreateView()
	{
		UnitEntityView unitEntityView = ViewSettings.Instantiate();
		if (unitEntityView != null)
		{
			unitEntityView.Blueprint = base.Blueprint;
		}
		unitEntityView.MarkCreatedAtRuntime();
		return unitEntityView;
	}

	protected override IEntityViewBase CreateViewForData()
	{
		try
		{
			return CreateView();
		}
		catch (Exception exception)
		{
			if (base.Blueprint != null)
			{
				PFLog.Default.ExceptionWithReport(exception, "Fail create view for " + base.Blueprint.name + ".");
			}
			else
			{
				PFLog.Default.ExceptionWithReport(exception, "Fail create view for [???] (UnitEntityData not have Blueprint).");
			}
			return null;
		}
	}

	public void Stop()
	{
		base.HoldState = false;
		base.Commands.InterruptAll((AbstractUnitCommand c) => !c.IsStarted);
		CombatState.LastTarget = null;
		CombatState.ManualTarget = null;
	}

	public void Hold()
	{
		base.HoldState = true;
		base.Commands.InterruptMove();
	}

	public void TryCancelCommands()
	{
		if (!base.Commands.IsRunning())
		{
			base.HoldState = false;
			base.Commands.InterruptAllInterruptible();
			CombatState.LastTarget = null;
			CombatState.ManualTarget = null;
			View.MovementAgent.Stop();
		}
	}

	public bool HasControlLossEffects()
	{
		foreach (Buff buff in base.Buffs)
		{
			foreach (EntityFactComponent component in buff.Components)
			{
				BlueprintComponent sourceBlueprintComponent = component.SourceBlueprintComponent;
				if (sourceBlueprintComponent is ChangeFaction)
				{
					return true;
				}
				if (sourceBlueprintComponent is AddCondition { Condition: UnitCondition.CantMove })
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsSummoned()
	{
		return base.Buffs.GetBuff(ConfigRoot.Instance.SystemMechanics.SummonedUnitBuff) != null;
	}

	public bool IsSummoned(out MechanicEntity caster)
	{
		caster = base.Buffs.GetBuff(ConfigRoot.Instance.SystemMechanics.SummonedUnitBuff)?.Context.MaybeCaster;
		if (caster != null)
		{
			return caster != this;
		}
		return false;
	}

	public bool IsUnseen()
	{
		return !Game.Instance.UnitGroups.Any((UnitGroup group) => group.IsEnemy(this) && group.Memory.ContainsVisible(this));
	}

	public bool IsCurrentUnit()
	{
		if (Game.Instance.Controllers.TurnController.CurrentUnit != null)
		{
			return this == Game.Instance.Controllers.TurnController.CurrentUnit;
		}
		return false;
	}

	public void PrepareRespec()
	{
	}

	public override void MarkExtra()
	{
		m_IsExtra = true;
		CombatGroup.Id = "<peaceful-unit>";
	}

	public void MarkSpawnFromPsychicPhenomena()
	{
		SpawnFromPsychicPhenomena = true;
	}

	public void MarkAsCloneOfMainCharacter()
	{
		m_IsCloneOfManeCharacter = true;
	}

	public void OnGainPathRank(BlueprintPath path)
	{
		Health.HitPoints.UpdateValue();
		EventBus.RaiseEvent((IBaseUnitEntity)this, (Action<IUnitGainPathRankHandler>)delegate(IUnitGainPathRankHandler h)
		{
			h.HandleUnitGainPathRank(path);
		}, isCheckRuntime: true);
	}

	protected override void OnDifficultyChanged()
	{
		foreach (ModifiableValue allStat in base.Stats.AllStats)
		{
			allStat.RecalculateBaseValue();
		}
	}

	protected override void DisposeImplementation()
	{
		bool isPreviewUnit = IsPreviewUnit;
		using (ContextData<DisableStatefulRandomContext>.RequestIf(isPreviewUnit))
		{
			using (ContextData<UnitHelper.DoNotCreateItems>.RequestIf(isPreviewUnit))
			{
				using (ContextData<UnitHelper.PreviewUnit>.RequestIf(isPreviewUnit))
				{
					base.DisposeImplementation();
				}
			}
		}
	}

	protected override void OnPreSave()
	{
		base.OnPreSave();
		if (Game.Instance.Controllers.UnitMovableAreaController.TryGetInitialPosition(this, out var initialPosition))
		{
			UnitPartDeploymentPhaseInitialPosition orCreate = GetOrCreate<UnitPartDeploymentPhaseInitialPosition>();
			if (orCreate != null)
			{
				orCreate.InitialPosition = initialPosition;
			}
		}
	}

	protected override void OnPrePostLoad()
	{
		base.OnPrePostLoad();
		UnitPartDeploymentPhaseInitialPosition optional = GetOptional<UnitPartDeploymentPhaseInitialPosition>();
		if (optional != null)
		{
			if (IsInCombat)
			{
				Game.Instance.Controllers.UnitMovableAreaController.ApplyInitialPosition(this, optional.InitialPosition);
			}
			Remove<UnitPartDeploymentPhaseInitialPosition>();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		float val2 = TimeToNextRoundTick;
		result.Append(ref val2);
		int val3 = CachedPerceptionRoll;
		result.Append(ref val3);
		if (LastRestTime.HasValue)
		{
			TimeSpan val4 = LastRestTime.Value;
			result.Append(ref val4);
		}
		List<BlueprintUnitUpgrader> appliedUpgraders = m_AppliedUpgraders;
		if (appliedUpgraders != null)
		{
			for (int i = 0; i < appliedUpgraders.Count; i++)
			{
				Hash128 val5 = SimpleBlueprintHasher.GetHash128(appliedUpgraders[i]);
				result.Append(ref val5);
			}
		}
		return result;
	}
}
