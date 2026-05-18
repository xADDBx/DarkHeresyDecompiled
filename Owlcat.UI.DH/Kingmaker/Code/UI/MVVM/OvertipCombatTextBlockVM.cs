using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Cheats;
using Kingmaker.Code.Framework.GameLog;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.Gameplay.Components;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.Mechanics.Utility.Damage;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Features.Concentration.Events;
using Kingmaker.Gameplay.Features.Morale;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipCombatTextBlockVM : ViewModel, IGlobalRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IGlobalRulebookSubscriber, IGlobalRulebookHandler<RulePerformSavingThrow>, IRulebookHandler<RulePerformSavingThrow>, IWarhammerAttackHandler, IHealingHandler, IDamageHandler, ICriticalEffectStageChanged, ISubscriber<IMechanicEntity>, ICriticalEffectStageChangeFailed, IMoraleValueHandler, IMoralePhaseHandler, IAttackOfOpportunityHandler<EntitySubscriber>, IAttackOfOpportunityHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IAttackOfOpportunityHandler, EntitySubscriber>, IUICultAmbushVisibilityChangeHandler, IUnitCustomCombatText, IEntityGainFactHandler<EntitySubscriber>, IEntityGainFactHandler, IEventTag<IEntityGainFactHandler, EntitySubscriber>, IEntityLostFactHandler<EntitySubscriber>, IEntityLostFactHandler, IEventTag<IEntityLostFactHandler, EntitySubscriber>, ITurnStartHandler<EntitySubscriber>, ITurnStartHandler, IEntitySubscriber, IEventTag<ITurnStartHandler, EntitySubscriber>, IConcentrationBrokenHandler
{
	private readonly ReactiveProperty<bool> m_HasActiveCombatMessage;

	private readonly MechanicEntityUIState m_EntityUIState;

	private readonly Dictionary<BlueprintFact, CombatMessageBase> m_AbilityMessages = new Dictionary<BlueprintFact, CombatMessageBase>();

	private readonly CombatTextMessageQueue<CombatMessageBase> m_MessagesQueue;

	private readonly ReactiveProperty<bool> m_IsForceHidden;

	private IDisposable m_AbilityMessagesTask;

	private int m_PendingMoraleDelta;

	private bool m_PendingHasCriticalEffect;

	private IDisposable m_MoraleDelayDisposable;

	private MechanicEntity MechanicEntity => m_EntityUIState.MechanicEntity.MechanicEntity;

	public int MessagesCount { get; private set; }

	public Observable<Unit> CombatMessageEnqueued => m_MessagesQueue.CombatMessageEnqueued;

	public ReadOnlyReactiveProperty<bool> HasActiveCombatMessage => m_HasActiveCombatMessage;

	public ReadOnlyReactiveProperty<bool> IsForceHidden => m_IsForceHidden;

	public OvertipCombatTextBlockVM(MechanicEntityUIState mechanicEntityUIState, ReactiveProperty<bool> hasActiveCombatMessage = null)
	{
		m_MessagesQueue = new CombatTextMessageQueue<CombatMessageBase>().AddTo(this);
		m_HasActiveCombatMessage = hasActiveCombatMessage ?? new ReactiveProperty<bool>().AddTo(this);
		m_EntityUIState = mechanicEntityUIState;
		if (m_EntityUIState.IsOvertipPartHiddenBySettings(UnitOvertipUIPart.CombatText))
		{
			m_IsForceHidden = new ReactiveProperty<bool>(value: false).AddTo(this);
			return;
		}
		m_IsForceHidden = new ReactiveProperty<bool>(GameUIState.Instance.GameMode.CurrentValue != GameModeType.Cutscene).AddTo(this);
		GameUIState.Instance.GameMode.Subscribe(delegate(GameModeType gameMode)
		{
			m_IsForceHidden.Value = gameMode == GameModeType.Cutscene;
		}).AddTo(this);
		m_MessagesQueue.AliveMessagesCount.Subscribe(delegate(int count)
		{
			MessagesCount = count;
			m_HasActiveCombatMessage.Value = count > 0;
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	public bool GetNextMessage(out CombatMessageBase message)
	{
		return m_MessagesQueue.Dequeue(out message);
	}

	public void ClearMessage(CombatMessageBase message)
	{
		m_MessagesQueue.ClearMessage(message);
	}

	public void ClearAllMessages()
	{
		m_MessagesQueue.ClearAllMessages();
	}

	IEntity IEntitySubscriber.GetSubscribingEntity()
	{
		return MechanicEntity;
	}

	public void HandleHealing(RuleHealDamage healDamage)
	{
		if (healDamage.Target == MechanicEntity)
		{
			int value = healDamage.Value;
			if (value != 0)
			{
				DamageStrategy strategy = healDamage.CalculateHealRule.Strategy;
				Sprite sprite = strategy switch
				{
					DamageStrategy.HealthOnly => ConfigRoot.Instance.UIConfig.UIIcons.Heal, 
					DamageStrategy.ArmorOnly => ConfigRoot.Instance.UIConfig.UIIcons.ArmorRepair, 
					_ => null, 
				};
				m_MessagesQueue.Enqueue(new CombatMessageHealing
				{
					Amount = value,
					Sprite = sprite,
					Strategy = strategy
				});
			}
		}
	}

	public void HandleDamageDealt(RuleDealDamage dealDamage)
	{
		if (!ContextData<GameLogDisabled>.Current && !dealDamage.IsGameLogDisabled && dealDamage.Target == MechanicEntity && !dealDamage.FromRuleWarhammerAttackRoll)
		{
			Sprite sprite;
			CombatMessageDamage.DamageBonus damageBonus = GetDamageBonus(dealDamage, out sprite);
			CombatMessageDamage messageBase = new CombatMessageDamage
			{
				Amount = dealDamage.ResultValue,
				Sprite = sprite,
				Bonus = damageBonus,
				IsImmune = false,
				SourcePosition = (dealDamage.Projectile?.LaunchPosition ?? dealDamage.Initiator?.Position ?? Vector3.one),
				TargetPosition = (dealDamage.Projectile?.CorePosition ?? dealDamage.Target?.Position ?? Vector3.one)
			};
			m_MessagesQueue.Enqueue(messageBase);
		}
	}

	public void HandleCriticalEffectStageChanged(BlueprintBodyPart bodyPart, int previous, int current)
	{
		if (EventInvokerExtensions.MechanicEntity != MechanicEntity || m_EntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue)
		{
			return;
		}
		if (previous > 0)
		{
			BlueprintBuff criticalEffectStageBuff = bodyPart.GetCriticalEffectStageBuff(previous);
			if (!TryCreateFromCombatTextsSettings(criticalEffectStageBuff, CombatTextEventType.OnDetach, isPersonal: false) && criticalEffectStageBuff != null)
			{
				PrepareAbilityMessagesToSend(criticalEffectStageBuff, new CombatMessageExpiredAbility
				{
					Name = criticalEffectStageBuff.Name,
					Sprite = criticalEffectStageBuff.Icon,
					Color = GetCriticalEffectStageColor(previous)
				});
			}
		}
		BlueprintBuff criticalEffectStageBuff2 = bodyPart.GetCriticalEffectStageBuff(current);
		if (criticalEffectStageBuff2 != null && !criticalEffectStageBuff2.IsHiddenInUI && !TryCreateFromCombatTextsSettings(criticalEffectStageBuff2, CombatTextEventType.OnAttach, isPersonal: false))
		{
			PrepareAbilityMessagesToSend(criticalEffectStageBuff2, new CombatMessageAbility
			{
				Name = criticalEffectStageBuff2.Name,
				Sprite = criticalEffectStageBuff2.Icon,
				Color = GetCriticalEffectStageColor(current)
			});
		}
	}

	public void HandleCriticalEffectStageChangeFailed(BlueprintBodyPart bodyPart, int failedStage)
	{
		if (EventInvokerExtensions.MechanicEntity == MechanicEntity)
		{
			BlueprintBuff criticalEffectStageBuff = bodyPart.GetCriticalEffectStageBuff(failedStage);
			if (criticalEffectStageBuff != null && !criticalEffectStageBuff.IsHiddenInUI)
			{
				m_MessagesQueue.Enqueue(new CombatMessageCriticalEffectCheckFailed
				{
					EffectName = criticalEffectStageBuff.Name
				});
			}
		}
	}

	public void HandleMoraleValueChanged(int delta, bool hasCriticalEffect)
	{
		if (delta != 0 && EventInvokerExtensions.MechanicEntity == MechanicEntity && !m_EntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue)
		{
			m_PendingMoraleDelta += delta;
			m_PendingHasCriticalEffect |= hasCriticalEffect;
			m_MoraleDelayDisposable?.Dispose();
			m_MoraleDelayDisposable = DelayedInvoker.InvokeInFrames(PushMoraleMessage, 10);
		}
	}

	private void PushMoraleMessage()
	{
		CombatMessageMorale messageBase = new CombatMessageMorale
		{
			Amount = m_PendingMoraleDelta,
			HasCriticalEffect = m_PendingHasCriticalEffect
		};
		m_MessagesQueue.Enqueue(messageBase);
		m_PendingMoraleDelta = 0;
		m_PendingHasCriticalEffect = false;
		m_MoraleDelayDisposable?.Dispose();
		m_MoraleDelayDisposable = null;
	}

	void IMoralePhaseHandler.HandleMoralePhaseChanged(MoralePhaseType phase)
	{
		if (EventInvokerExtensions.MechanicEntity == MechanicEntity && !m_EntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue)
		{
			m_MessagesQueue.Enqueue(new CombatMessageMoralePhase
			{
				PhaseType = phase
			});
		}
	}

	public void HandleAttackOfOpportunity(BaseUnitEntity target)
	{
		m_MessagesQueue.Enqueue(new CombatMessageAttackOfOpportunity());
	}

	public void OnEventAboutToTrigger(RulePerformSavingThrow evt)
	{
	}

	public void OnEventDidTrigger(RulePerformSavingThrow evt)
	{
		if (Rulebook.CurrentContext.Current != null && Rulebook.CurrentContext.Current.Initiator == MechanicEntity)
		{
			RuleDealDamage ruleDealDamage = Rulebook.CurrentContext.LastEventOfType<RuleDealDamage>();
			if (evt.IsPassed || ruleDealDamage?.Initiator != evt.Initiator)
			{
				m_MessagesQueue.Enqueue(new CombatMessageSavingThrow
				{
					Passed = evt.IsPassed,
					Reason = evt.Reason.Name,
					Sprite = evt.Reason.Icon,
					StatType = evt.StatType,
					Roll = -1,
					DC = -1
				});
			}
		}
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		if (Rulebook.CurrentContext.Current.Initiator == MechanicEntity && !evt.Ability.Blueprint.DisableLog && !m_EntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue && !TryCreateFromCombatTextsSettings(evt.Ability?.Blueprint?.OriginalBlueprint, CombatTextEventType.OnAttach, isPersonal: true) && Rulebook.CurrentContext.Current != null && evt.Ability?.Blueprint != null)
		{
			m_MessagesQueue.Enqueue(new CombatMessageAbilityReplaceable(evt.Ability.Blueprint.Name, evt.Ability.Blueprint.Icon));
		}
	}

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	public void HandleAttack(RulePerformAttack withWeaponAttackHit)
	{
		RuleDealDamage resultDamageRule = withWeaponAttackHit.ResultDamageRule;
		if ((resultDamageRule == null || !resultDamageRule.IsFake) && withWeaponAttackHit.Target == MechanicEntity)
		{
			if (withWeaponAttackHit.ResultIsHit)
			{
				WarhammerAttackHit(withWeaponAttackHit);
				return;
			}
			m_MessagesQueue.Enqueue(new CombatMessageAttackMiss
			{
				Result = withWeaponAttackHit.Result,
				IsCasterCriticallyInjured = withWeaponAttackHit.RollPerformAttackRule.MissCausedByCritOnSelf,
				SourcePosition = (withWeaponAttackHit.Projectile?.LaunchPosition ?? withWeaponAttackHit.Initiator?.Position ?? Vector3.one),
				TargetPosition = (withWeaponAttackHit.Projectile?.CorePosition ?? withWeaponAttackHit.Target?.Position ?? Vector3.one)
			});
		}
	}

	public void HandleCustomCombatText(LocalizedString text)
	{
		if (EventInvokerExtensions.MechanicEntity == MechanicEntity)
		{
			m_MessagesQueue.Enqueue(new CombatMessageCustom
			{
				Text = text.Text
			});
		}
	}

	public void HandleCultAmbushVisibilityChange()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && baseUnitEntity == MechanicEntity)
		{
			m_MessagesQueue.Enqueue(new CombatMessageCultAmbush());
		}
	}

	public void HandleEntityGainFact(EntityFact fact)
	{
		if (fact == null || fact.GetSubscribingEntity() != MechanicEntity || MechanicEntity.IsDeadOrUnconscious)
		{
			return;
		}
		Buff buff = fact as Buff;
		if ((buff == null || (!buff.Hidden && !buff.Blueprint.CriticalEffect)) && (!(fact.Blueprint is BlueprintAbility) || fact.SourceFact != null))
		{
			bool isPersonal = buff?.SourceFact != null && buff.SourceFact.Owner == MechanicEntity;
			if (!TryCreateFromCombatTextsSettings(fact.Blueprint as BlueprintMechanicEntityFact, CombatTextEventType.OnAttach, isPersonal) && buff != null)
			{
				CombatMessageBase message = ((buff.Blueprint.Stacking == StackingType.Replace) ? ((CombatMessageBase)new CombatMessageAbilityReplaceable(buff.Name, buff.Icon)) : ((CombatMessageBase)new CombatMessageAbility
				{
					Name = buff.Name,
					Sprite = buff.Icon
				}));
				PrepareAbilityMessagesToSend(fact.Blueprint, message);
			}
		}
	}

	public void HandleEntityLostFact(EntityFact fact)
	{
		if (fact == null || fact.GetSubscribingEntity() != MechanicEntity || MechanicEntity.IsDeadOrUnconscious)
		{
			return;
		}
		Buff buff = fact as Buff;
		if (buff == null || (!buff.Hidden && !buff.Blueprint.CriticalEffect))
		{
			bool isPersonal = buff?.SourceFact != null && buff.SourceFact.Owner == MechanicEntity;
			if (!TryCreateFromCombatTextsSettings(fact.Blueprint as BlueprintMechanicEntityFact, CombatTextEventType.OnDetach, isPersonal) && buff != null)
			{
				PrepareAbilityMessagesToSend(fact.Blueprint, new CombatMessageExpiredAbility
				{
					Name = buff.Name,
					Sprite = buff.Icon
				});
			}
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		MechanicEntity mechanicEntity = MechanicEntity;
		if (!mechanicEntity.CanAct)
		{
			BlueprintBuff disabledCommonBuff = ConfigRoot.Instance.SystemMechanics.DisabledCommonBuff;
			if (mechanicEntity.Buffs?.GetBuff(disabledCommonBuff) != null)
			{
				m_MessagesQueue.Enqueue(new CombatMessageAbility
				{
					Name = disabledCommonBuff.Name,
					Sprite = disabledCommonBuff.Icon,
					BigSprite = ConfigRoot.Instance.UIConfig.UIIcons.CantAct
				});
			}
		}
	}

	void IConcentrationBrokenHandler.HandleConcentrationBroken(MechanicEntity reason)
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (mechanicEntity != null && mechanicEntity == MechanicEntity)
		{
			m_MessagesQueue.Enqueue(new CombatMessageCustom
			{
				Text = UIStrings.Instance.CombatTexts.ConcentrationBroken,
				Color = UIConfig.Instance.CombatTextColors.ConcentrationBrokenColor
			});
		}
	}

	protected override void OnDispose()
	{
		m_HasActiveCombatMessage.Value = false;
	}

	private void WarhammerAttackHit(RulePerformAttack withWeaponAttackHit)
	{
		RuleDealDamage resultDamageRule = withWeaponAttackHit.ResultDamageRule;
		if (!m_EntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue && resultDamageRule != null && !TryCreateFromCombatTextsSettings(withWeaponAttackHit.Ability.Blueprint.BaseAbility, CombatTextEventType.OnAttach, isPersonal: false))
		{
			Sprite sprite;
			CombatMessageDamage.DamageBonus damageBonus = GetDamageBonus(resultDamageRule, out sprite);
			CombatMessageDamage messageBase = new CombatMessageDamage
			{
				Amount = withWeaponAttackHit.ResultDamageValue,
				Sprite = sprite,
				Bonus = damageBonus,
				IsImmune = false,
				SourcePosition = (resultDamageRule.Projectile?.LaunchPosition ?? resultDamageRule.Initiator?.Position ?? Vector3.one),
				TargetPosition = (resultDamageRule.Projectile?.CorePosition ?? resultDamageRule.Target?.Position ?? Vector3.one)
			};
			m_MessagesQueue.Enqueue(messageBase);
		}
	}

	private CombatMessageDamage.DamageBonus GetDamageBonus(RuleDealDamage dealDamage, out Sprite sprite)
	{
		RolledDamage resultDamage = dealDamage.ResultDamage;
		if (resultDamage.IsVitalDamage)
		{
			sprite = ConfigRoot.Instance.UIConfig.UIIcons.VitalDamageBonus;
			return CombatMessageDamage.DamageBonus.Vital;
		}
		if (dealDamage.ResultIsCritical)
		{
			sprite = ConfigRoot.Instance.UIConfig.UIIcons.CriticalApplied;
			return CombatMessageDamage.DamageBonus.Critical;
		}
		int resultDamageToHealthValue = resultDamage.ResultDamageToHealthValue;
		int resultBonusDamageToHealth = resultDamage.ResultBonusDamageToHealth;
		if (resultDamageToHealthValue > 0 && resultBonusDamageToHealth > 0)
		{
			sprite = ConfigRoot.Instance.UIConfig.UIIcons.HPDamageBonus;
			return CombatMessageDamage.DamageBonus.Hp;
		}
		int resultDamageToArmorValue = resultDamage.ResultDamageToArmorValue;
		int resultBonusDamageToArmor = resultDamage.ResultBonusDamageToArmor;
		if (resultDamageToArmorValue > 0 && resultBonusDamageToArmor > 0)
		{
			sprite = ConfigRoot.Instance.UIConfig.UIIcons.ArmorDamageBonus;
			return CombatMessageDamage.DamageBonus.Armor;
		}
		sprite = null;
		return CombatMessageDamage.DamageBonus.None;
	}

	private void PrepareAbilityMessagesToSend(BlueprintFact fact, CombatMessageBase message)
	{
		m_AbilityMessages[fact] = message;
		m_AbilityMessagesTask?.Dispose();
		m_AbilityMessagesTask = DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(delegate
		{
			if (MechanicEntity.IsDeadOrUnconscious)
			{
				m_AbilityMessages.Clear();
				m_AbilityMessagesTask?.Dispose();
			}
			else
			{
				foreach (var (blueprintFact2, messageBase) in m_AbilityMessages)
				{
					if (CheatsCombat.CombatTextDebugEnabled)
					{
						PFLog.UI.Log(m_EntityUIState.MechanicEntity.Name + ": " + blueprintFact2.name);
					}
					m_MessagesQueue.Enqueue(messageBase);
				}
				m_AbilityMessages.Clear();
				m_AbilityMessagesTask?.Dispose();
			}
		});
	}

	private bool TryCreateFromCombatTextsSettings(BlueprintMechanicEntityFact blueprint, CombatTextEventType eventType, bool isPersonal)
	{
		if (blueprint?.CombatTextSettings == null)
		{
			return blueprint is BlueprintFeature;
		}
		CombatTextCaseSettings combatTextCaseSettings = blueprint.CombatTextSettings?.GetSettings(GetCombatTextTargetType(isPersonal), eventType);
		if (combatTextCaseSettings != null)
		{
			CombatTextSettings combatTextSettings = blueprint.CombatTextSettings;
			if (combatTextSettings != null && !combatTextSettings.HideCombatText)
			{
				string name = (combatTextCaseSettings.UseNameOfAbility ? blueprint.Name : (combatTextCaseSettings.Text?.Text ?? string.Empty));
				Sprite sprite = (combatTextCaseSettings.UseAbilityIcon ? blueprint.Icon : ((combatTextCaseSettings.Icon != null) ? blueprint.Icon : null));
				Color color = UIConfig.Instance.CombatTextColors.GetColor(combatTextCaseSettings.MessageColorColorType);
				CombatMessageBase message = ((eventType == CombatTextEventType.OnDetach) ? ((CombatMessageBase)new CombatMessageExpiredAbility
				{
					Name = name,
					Sprite = sprite,
					Color = color
				}) : ((CombatMessageBase)new CombatMessageAbility
				{
					Name = name,
					Sprite = sprite,
					Color = color
				}));
				PrepareAbilityMessagesToSend(blueprint, message);
				return true;
			}
		}
		if (CheatsCombat.CombatTextDebugEnabled)
		{
			return false;
		}
		return true;
	}

	private CombatTextTargetType GetCombatTextTargetType(bool isPersonal)
	{
		if (MechanicEntity is MapObjectEntity)
		{
			return CombatTextTargetType.MapObject;
		}
		if (isPersonal)
		{
			return CombatTextTargetType.Personal;
		}
		if (m_EntityUIState.IsEnemy.CurrentValue)
		{
			return CombatTextTargetType.Enemy;
		}
		if (m_EntityUIState.IsPlayerFaction.CurrentValue)
		{
			return CombatTextTargetType.Ally;
		}
		return CombatTextTargetType.NPC;
	}

	private Color? GetCriticalEffectStageColor(int stage)
	{
		return stage switch
		{
			1 => ConfigRoot.Instance.UIConfig.CombatTextColors.NegativeLow, 
			2 => ConfigRoot.Instance.UIConfig.CombatTextColors.NegativeMiddle, 
			3 => ConfigRoot.Instance.UIConfig.CombatTextColors.NegativeHigh, 
			_ => null, 
		};
	}
}
