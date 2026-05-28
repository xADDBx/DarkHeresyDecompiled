using System;
using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.Gameplay.Components;
using Kingmaker.Code.Gameplay.Controllers.Combat;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Predictions;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Mechanics.Entities;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoVM : ViewModel, IUnitDirectHoverUIHandler, ISubscriber, IUnitInfoDetailsUIHandler, IAbilityTargetSelectionUIHandler, IDamageHandler, IHealingHandler, IActorStatChangedHandler, ISubscriber<IMechanicEntity>
{
	private HashSet<AbilityUIGroup> m_AbilityUIGroups = new HashSet<AbilityUIGroup>();

	private readonly List<UnitInfoAbilityVM> m_AbilitiesList = new List<UnitInfoAbilityVM>();

	private readonly ReactiveProperty<IReadOnlyList<UnitInfoAbilityVM>> m_Abilities;

	private readonly ReactiveProperty<bool> m_IsHover = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsPreciseAttack = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsDirtyContent = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasAbility = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasCompareData = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<Vector3> m_Position = new ReactiveProperty<Vector3>(Vector3.zero);

	private readonly ReactiveProperty<bool> m_HasMorale = new ReactiveProperty<bool>();

	private readonly PreciseAttackController m_PreciseAttackController;

	private readonly CasterBuffsInfoBaseVM<UnitBuffUIInfo> m_CasterDamageBuffs;

	private MechanicEntityUIState m_MechanicEntityUIState;

	private UnitUIInspectSettings m_InspectSettings;

	private IDisposable m_CollectAbilityPropertySubscription;

	private IDisposable m_MoraleSubscription;

	private IDisposable m_MoralePredictionSubscription;

	private IDisposable m_ConcentrationSubscription;

	private AbilityData m_Ability;

	private Vector3 m_EntityPosition;

	public readonly string ConcentrationTitle;

	public readonly UnitInfoBuffBlockVM BuffBlockVM;

	public readonly UnitInfoReactiveData Data = new UnitInfoReactiveData(isCompareData: false);

	public readonly UnitInfoReactiveData CompareData = new UnitInfoReactiveData(isCompareData: true);

	public readonly Observable<Unit> BodyPartChanged;

	public ReadOnlyReactiveProperty<IReadOnlyList<UnitInfoAbilityVM>> Abilities => m_Abilities;

	public MechanicEntity Unit => m_MechanicEntityUIState?.MechanicEntity.MechanicEntity;

	public MechanicEntityUIWrapper? UnitWrapper => m_MechanicEntityUIState?.MechanicEntity;

	public HUDContext HUDContext => RootVM.Instance.HUDContext;

	public ReadOnlyReactiveProperty<bool> IsHover => m_IsHover;

	public ReadOnlyReactiveProperty<bool> IsPreciseAttack => m_IsPreciseAttack;

	public ReadOnlyReactiveProperty<bool> IsDirtyContent => m_IsDirtyContent;

	public ReadOnlyReactiveProperty<bool> HasAbility => m_HasAbility;

	public ReadOnlyReactiveProperty<bool> HasCompareData => m_HasCompareData;

	public ReadOnlyReactiveProperty<Vector3> Position => m_Position;

	public ReadOnlyReactiveProperty<bool> HasMorale => m_HasMorale;

	public Observable<CollectionAddEvent<UnitBuffUIInfo>> DamageBuffOnCasterAdded => m_CasterDamageBuffs.ObserveBuffAdded();

	public Observable<Unit> DamageBuffsOnCasterCleared => m_CasterDamageBuffs.ObserveBuffsCleared();

	public bool HideRealHealthInUI
	{
		get
		{
			if (Unit != null)
			{
				return Unit.HasMechanicFeature(MechanicsFeatureType.HideRealHealthInUI);
			}
			return false;
		}
	}

	public bool IsCountHpAsArmor => m_MechanicEntityUIState?.IsCountHpAsArmor ?? false;

	public int HealthDamage
	{
		get
		{
			if (m_MechanicEntityUIState == null)
			{
				return 0;
			}
			return m_MechanicEntityUIState.AbilityTargetUIData.CurrentValue.Damage.HealthMaxDamage;
		}
	}

	public UnitInfoVM(PreciseAttackController preciseAttackController)
	{
		m_Abilities = new ReactiveProperty<IReadOnlyList<UnitInfoAbilityVM>>(m_AbilitiesList).AddTo(this);
		ConcentrationTitle = UIStrings.Instance.UnitInfo.ConcentrationTitle;
		m_PreciseAttackController = preciseAttackController;
		m_PreciseAttackController.Target.Subscribe(UpdatePreciseAttackTarget).AddTo(this);
		m_PreciseAttackController.SelectedBodyPart.Subscribe(UpdatePreciseAttackSelectedBodyPart).AddTo(this);
		m_PreciseAttackController.HoveredBodyPart.Subscribe(UpdatePreciseAttackHoveredBodyPart).AddTo(this);
		BodyPartChanged = m_PreciseAttackController.SelectedBodyPart.AsUnitObservable();
		BuffBlockVM = new UnitInfoBuffBlockVM(null).AddTo(this);
		m_CasterDamageBuffs = new CasterDamageBuffsInfoVM().AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		OwlcatR3UnitExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate), delegate
		{
			m_Position.Value = GetEntityPosition();
		}).AddTo(this);
	}

	public bool HasSettingsFlags(UnitInspectUIFlags flags)
	{
		return m_InspectSettings?.HasFlags(flags) ?? false;
	}

	public int GetCritsThroughArmor()
	{
		return ((m_MechanicEntityUIState?.AbilityTargetUIData?.CurrentValue)?.Ability?.Weapon)?.Blueprint.CritsThroughArmor ?? 0;
	}

	public int GetCriticalEffectChance(BlueprintBodyPart bodyPart, int index, out bool alreadyApplied)
	{
		AbilityTargetUIData? abilityTargetUIData = m_MechanicEntityUIState?.AbilityTargetUIData?.CurrentValue;
		if (!abilityTargetUIData.HasValue)
		{
			alreadyApplied = false;
			return 0;
		}
		int num = index + 1;
		int num2 = 0;
		PartHealth health = m_MechanicEntityUIState.MechanicEntity.Health;
		if (health != null)
		{
			num2 = health.GetCriticalStage(bodyPart);
		}
		alreadyApplied = num2 >= num;
		int healthMaxDamage = abilityTargetUIData.Value.Damage.HealthMaxDamage;
		int critsThroughArmor = GetCritsThroughArmor();
		if (healthMaxDamage > 0 || num <= num2 + critsThroughArmor)
		{
			float rawCriticalEffectChance = m_PreciseAttackController.GetRawCriticalEffectChance(bodyPart, index);
			return Mathf.Max(0, Mathf.FloorToInt(rawCriticalEffectChance));
		}
		return 0;
	}

	public void SetDirtyContent(bool isDirty)
	{
		m_IsDirtyContent.Value = isDirty;
	}

	public void CollectAdditionalEffects(BlueprintBodyPart bodyPart, List<string> output)
	{
		output.Clear();
		if (bodyPart == null)
		{
			return;
		}
		if (bodyPart.CanBreakTargetConcentrationIfHit(Unit, checkTargetHasConcentration: false))
		{
			output.Add(LocalizedTexts.Instance.PreciseAttack.CanBreakTargetConcentrationIfHit);
		}
		if (bodyPart.CanChangeTargetTurnOrderIfHit())
		{
			output.Add(LocalizedTexts.Instance.PreciseAttack.CanChangeTargetTurnOrderIfHit);
		}
		LocalizedString description = bodyPart.Description;
		if (description == null || description.Empty)
		{
			return;
		}
		using (GameLogContext.Scope)
		{
			GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)Unit;
			output.Add(bodyPart.Description.Text);
		}
	}

	public void HandleHoverChange(AbstractUnitEntityView unitEntityView, bool isHover, bool isDirect)
	{
		if (!IsPreciseAttack.CurrentValue)
		{
			m_IsHover.Value = isHover && isDirect;
			if (isHover)
			{
				SetMechanicEntityUIState(unitEntityView.Data);
			}
		}
	}

	public void HandleUnitManual(MechanicEntity mechanicEntity)
	{
		if (!IsPreciseAttack.CurrentValue)
		{
			SetMechanicEntityUIState(mechanicEntity);
			m_IsHover.Value = true;
		}
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		m_Ability = ability;
		m_HasAbility.Value = true;
		Data.SetAbilityIcon(m_Ability.Icon);
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		m_Ability = null;
		m_HasAbility.Value = false;
		Data.SetAbilityIcon(null);
	}

	public void HandleDamageDealt(RuleDealDamage dealDamage)
	{
		Data.UpdateHealth(m_MechanicEntityUIState);
	}

	public void HandleHealing(RuleHealDamage healDamage)
	{
		Data.UpdateHealth(m_MechanicEntityUIState);
	}

	public void HandleActorStatChanged(StatChangeSet stats)
	{
		if (EventInvokerExtensions.MechanicEntity == Unit && stats.Contains(StatType.MaxHitPoints))
		{
			Data.UpdateHealth(m_MechanicEntityUIState);
		}
	}

	public bool HasAbilityGroup(AbilityUIGroup group)
	{
		return m_AbilityUIGroups.Contains(group);
	}

	protected override void OnDispose()
	{
		SetMechanicEntityUIState(null);
	}

	private void SetMechanicEntityUIState(MechanicEntity mechanicEntity)
	{
		m_CollectAbilityPropertySubscription?.Dispose();
		m_MoraleSubscription?.Dispose();
		m_MoralePredictionSubscription?.Dispose();
		m_ConcentrationSubscription?.Dispose();
		if (mechanicEntity == null)
		{
			m_MechanicEntityUIState = null;
			m_InspectSettings = null;
			m_CollectAbilityPropertySubscription = null;
			m_MoraleSubscription = null;
			m_MoralePredictionSubscription = null;
			m_ConcentrationSubscription = null;
			Data.ClearFields();
			CompareData.ClearFields();
			ClearAbilities(notify: true);
			BuffBlockVM.SetUnitData(null);
			m_CasterDamageBuffs.SetTargetEntity(null);
			return;
		}
		m_MechanicEntityUIState = UnitUIStateHolder.Instance.GetOrCreateUnitState(mechanicEntity);
		m_InspectSettings = m_MechanicEntityUIState.GetBlueprintComponent<UnitUISettings>()?.InspectSettings;
		m_CasterDamageBuffs.SetTargetEntity(m_MechanicEntityUIState);
		BuffBlockVM.SetUnitData(Unit);
		UpdateAbilities();
		Data.UpdateCommon(m_MechanicEntityUIState);
		Data.UpdateHealth(m_MechanicEntityUIState);
		UpdateMorale();
		UpdateConcentration();
		m_CollectAbilityPropertySubscription = m_MechanicEntityUIState.IsInCombat.CombineLatest(m_MechanicEntityUIState.IsVisibleForPlayer, m_MechanicEntityUIState.IsDeadOrUnconsciousIsDead, m_MechanicEntityUIState.Ability, m_MechanicEntityUIState.IsMouseOverUnit, m_MechanicEntityUIState.IsAoETarget, m_MechanicEntityUIState.AbilityTargetUIData, m_PreciseAttackController.SelectedBodyPart, (bool isInCombat, bool isVisibleForPlayer, bool isDead, AbilityData ability, bool isHover, bool isAoETarget, AbilityTargetUIData abilityTargetUIData, PreciseAttackController.BodyPartUIData _) => new { isInCombat, isVisibleForPlayer, isDead, ability, isHover, isAoETarget, abilityTargetUIData }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(value =>
		{
			bool show = value.isInCombat && value.ability != null && (value.isVisibleForPlayer || value.ability.IsPrecise) && !value.isDead && (value.isHover || value.isAoETarget || value.ability.IsPrecise);
			Data.CollectAbilityProperty(m_MechanicEntityUIState, value.ability, value.ability?.IsPrecise ?? false, show);
		});
	}

	private void UpdatePreciseAttackTarget(MechanicEntity target)
	{
		if (target == null)
		{
			m_IsPreciseAttack.Value = false;
			m_IsHover.Value = false;
			return;
		}
		m_IsPreciseAttack.Value = true;
		SetMechanicEntityUIState(target);
		Data.UpdateBodyPartData(m_MechanicEntityUIState, m_Ability, HUDContext.PointerWorldPosition, null);
		CompareData.UpdateBodyPartData(m_MechanicEntityUIState, m_Ability, HUDContext.PointerWorldPosition, null);
	}

	private void UpdatePreciseAttackSelectedBodyPart(PreciseAttackController.BodyPartUIData bodyPartUIData)
	{
		if (bodyPartUIData != null)
		{
			Data.UpdateBodyPartData(m_MechanicEntityUIState, m_Ability, HUDContext.PointerWorldPosition, bodyPartUIData);
			UpdatePreciseAttackHoveredBodyPart(bodyPartUIData);
		}
	}

	private void UpdatePreciseAttackHoveredBodyPart(PreciseAttackController.BodyPartUIData bodyPartUIData)
	{
		if (bodyPartUIData == null || bodyPartUIData == m_PreciseAttackController.SelectedBodyPart.CurrentValue)
		{
			CompareData.ClearFields();
			m_HasCompareData.Value = false;
		}
		else
		{
			CompareData.UpdateBodyPartData(m_MechanicEntityUIState, m_Ability, HUDContext.PointerWorldPosition, bodyPartUIData);
			m_HasCompareData.Value = true;
		}
	}

	private void UpdateMorale()
	{
		m_HasMorale.Value = Unit is BaseUnitEntity baseUnitEntity && !baseUnitEntity.Features.DoNotUseMoraleAndPowerBalance;
		if (m_HasMorale.CurrentValue)
		{
			m_MoraleSubscription = ObservableSubscribeExtensions.Subscribe(m_MechanicEntityUIState.MoraleChanged.DebounceFrame(1, UnityFrameProvider.PreLateUpdate), delegate
			{
				Data.UpdateMorale(m_MechanicEntityUIState);
			}).AddTo(this);
			m_MoralePredictionSubscription = m_MechanicEntityUIState.MoralePrediction.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate(IUIUnitMoraleData prediction)
			{
				Data.UpdateMoralePrediction(m_MechanicEntityUIState, prediction);
			}).AddTo(this);
			Data.UpdateMorale(m_MechanicEntityUIState);
		}
	}

	private void UpdateConcentration()
	{
		m_ConcentrationSubscription = m_MechanicEntityUIState.ConcentrationBuff.Subscribe(Data.UpdateConcentrationBuff).AddTo(this);
	}

	private Vector3 GetEntityPosition()
	{
		if (m_MechanicEntityUIState == null)
		{
			return m_EntityPosition;
		}
		Vector3 vector = ((m_MechanicEntityUIState.MechanicEntity.MechanicEntity?.Blueprint)?.GetComponent<UnitUISettings>())?.InspectSettings.UnitInfoOffset ?? Vector3.zero;
		Transform bone = m_MechanicEntityUIState.GetBone();
		if (bone != null && !m_MechanicEntityUIState.MechanicEntity.IsDeadOrUnconscious)
		{
			m_EntityPosition = bone.position;
			m_EntityPosition.y += 0.25f;
		}
		else
		{
			MechanicEntityUIWrapper mechanicEntity = m_MechanicEntityUIState.MechanicEntity;
			if (mechanicEntity.IsDeadOrUnconscious && mechanicEntity.IsDeadAndHasAttachedDroppedLoot)
			{
				m_EntityPosition = m_MechanicEntityUIState.MechanicEntity.MechanicEntity.GetOptional<PartInventory>()?.AttachedDroppedLootData.Position ?? Vector3.zero;
				m_EntityPosition.y += Vector2.zero.y;
			}
			else if (m_MechanicEntityUIState.MechanicEntity.IsDeadOrUnconscious && Unit is BaseUnitEntity baseUnitEntity)
			{
				m_EntityPosition = baseUnitEntity.View?.CorpseOvertipPosition ?? baseUnitEntity.Position;
			}
			else if (Unit is AbstractUnitEntity abstractUnitEntity)
			{
				m_EntityPosition = abstractUnitEntity.Position;
				m_EntityPosition.y += abstractUnitEntity.View?.CameraOrientedBoundsSize.y ?? Vector2.zero.y;
			}
			else
			{
				m_EntityPosition = Vector3.zero;
			}
		}
		m_EntityPosition += vector;
		return m_EntityPosition;
	}

	private void UpdateAbilities()
	{
		ClearAbilities(notify: false);
		if (Unit is BaseUnitEntity { IsInPlayerParty: false } baseUnitEntity)
		{
			BlueprintAbility[] array = (from a in UIUtilityUnit.CollectAbilities(baseUnitEntity)
				select a.Blueprint).ToArray();
			foreach (BlueprintAbility abilityBlueprint in array)
			{
				m_AbilitiesList.Add(new UnitInfoAbilityVM(abilityBlueprint, AbilityUIGroup.Active));
				m_AbilityUIGroups.Add(AbilityUIGroup.Active);
			}
			UIFeature[] array2 = UIUtilityUnit.CollectFeats(baseUnitEntity).ToArray();
			foreach (UIFeature uIFeature in array2)
			{
				m_AbilitiesList.Add(new UnitInfoAbilityVM(uIFeature.Feature, AbilityUIGroup.Passive));
				m_AbilityUIGroups.Add(AbilityUIGroup.Passive);
			}
			m_Abilities.ForceNotify();
		}
		else
		{
			m_Abilities.ForceNotify();
		}
	}

	private void ClearAbilities(bool notify)
	{
		m_AbilitiesList.Clear();
		m_AbilityUIGroups.Clear();
		if (notify && !m_Abilities.IsDisposed)
		{
			m_Abilities.ForceNotify();
		}
	}
}
