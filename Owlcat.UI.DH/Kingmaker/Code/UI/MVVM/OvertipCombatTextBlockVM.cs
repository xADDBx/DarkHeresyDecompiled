using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.Code.Framework.GameLog;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.Gameplay.Components;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.Mechanics.Utility.Damage;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Features.Morale;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipCombatTextBlockVM : ViewModel, IGlobalRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IGlobalRulebookSubscriber, IGlobalRulebookHandler<RulePerformSavingThrow>, IRulebookHandler<RulePerformSavingThrow>, IWarhammerAttackHandler, IHealingHandler, IDamageHandler, ICriticalEffectStageChanged, ISubscriber<IMechanicEntity>, ICriticalEffectStageChangeFailed, IMoraleValueHandler, IMoralePhaseHandler, IAttackOfOpportunityHandler<EntitySubscriber>, IAttackOfOpportunityHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IAttackOfOpportunityHandler, EntitySubscriber>, IUICultAmbushVisibilityChangeHandler, IUnitCustomCombatText, IEntityGainFactHandler<EntitySubscriber>, IEntityGainFactHandler, IEventTag<IEntityGainFactHandler, EntitySubscriber>, IEntityLostFactHandler<EntitySubscriber>, IEntityLostFactHandler, IEventTag<IEntityLostFactHandler, EntitySubscriber>, ITurnStartHandler<EntitySubscriber>, ITurnStartHandler, IEntitySubscriber, IEventTag<ITurnStartHandler, EntitySubscriber>
{
	private readonly ReactiveCommand<CombatMessageBase> m_CombatMessage;

	private readonly ReactiveProperty<bool> m_HasActiveCombatMessage;

	private readonly MechanicEntityUIState m_MechanicEntityUIState;

	private readonly Dictionary<BlueprintFact, CombatMessageBase> m_AbilityMessages = new Dictionary<BlueprintFact, CombatMessageBase>();

	private IDisposable m_AbilityMessagesTask;

	private int m_PendingMoraleDelta;

	private bool m_PendingHasCriticalEffect;

	private IDisposable m_MoraleDelayDisposable;

	private MechanicEntity m_MechanicEntity => m_MechanicEntityUIState.MechanicEntity.MechanicEntity;

	public bool IsForbiddenToShow => GameUIState.Instance.GameMode.Value == GameModeType.Cutscene;

	public Observable<CombatMessageBase> CombatMessage => m_CombatMessage;

	public ReadOnlyReactiveProperty<bool> HasActiveCombatMessage => m_HasActiveCombatMessage;

	public OvertipCombatTextBlockVM(MechanicEntityUIState mechanicEntityUIState, ReactiveProperty<bool> hasActiveCombatMessage = null)
	{
		m_CombatMessage = new ReactiveCommand<CombatMessageBase>().AddTo(this);
		m_HasActiveCombatMessage = hasActiveCombatMessage ?? new ReactiveProperty<bool>().AddTo(this);
		m_MechanicEntityUIState = mechanicEntityUIState;
		EventBus.Subscribe(this).AddTo(this);
	}

	public void SetActiveMessagesCount(int messagesCount)
	{
		m_HasActiveCombatMessage.Value = messagesCount > 0;
	}

	public IEntity GetSubscribingEntity()
	{
		return m_MechanicEntity;
	}

	public void HandleHealing(RuleHealDamage healDamage)
	{
		if (healDamage.Target != m_MechanicEntity)
		{
			return;
		}
		int value = healDamage.Value;
		if (value != 0)
		{
			Sprite sprite = null;
			if (Rulebook.CurrentContext.First is RulePerformAbility rulePerformAbility)
			{
				sprite = rulePerformAbility.Ability.Blueprint.Icon;
			}
			m_CombatMessage.Execute(new CombatMessageHealing
			{
				Amount = value,
				Sprite = sprite
			});
		}
	}

	public void HandleDamageDealt(RuleDealDamage dealDamage)
	{
		if ((bool)ContextData<GameLogDisabled>.Current || dealDamage.IsGameLogDisabled || dealDamage.Target != m_MechanicEntity || dealDamage.FromRuleWarhammerAttackRoll)
		{
			return;
		}
		Sprite sprite = null;
		if (Rulebook.CurrentContext.First is RulePerformAbility rulePerformAbility)
		{
			sprite = rulePerformAbility.Ability.Blueprint.Icon;
		}
		if (sprite == null)
		{
			if (dealDamage.Reason.Ability != null)
			{
				sprite = dealDamage.Reason.Ability.Blueprint.Icon;
			}
			else if (dealDamage.Reason.Fact != null)
			{
				sprite = (dealDamage.Reason.Fact.Blueprint as BlueprintAbility)?.Icon;
				if (sprite == null)
				{
					sprite = (dealDamage.Reason.Fact.Blueprint as BlueprintActivatableAbility)?.Icon;
				}
				if (sprite == null)
				{
					sprite = (dealDamage.Reason.Fact.Blueprint as BlueprintBuff)?.Icon;
				}
			}
		}
		bool hasCriticalEffect = false;
		EntityFactSource entityFactSource = dealDamage.Reason.Fact?.FirstSource;
		if (entityFactSource != null && entityFactSource.Blueprint is BlueprintBuff { CriticalEffect: not false } blueprintBuff)
		{
			hasCriticalEffect = true;
			sprite = blueprintBuff.Icon;
		}
		CombatMessageDamage parameter = new CombatMessageDamage
		{
			Amount = dealDamage.ResultValue,
			Sprite = sprite,
			IsVital = dealDamage.ResultDamage.IsVitalDamage,
			IsImmune = false,
			HasCriticalEffect = hasCriticalEffect,
			SourcePosition = (dealDamage.Projectile?.LaunchPosition ?? dealDamage.Initiator?.Position ?? Vector3.one),
			TargetPosition = (dealDamage.Projectile?.CorePosition ?? dealDamage.Target?.Position ?? Vector3.one)
		};
		m_CombatMessage.Execute(parameter);
	}

	public void HandleCriticalEffectStageChanged(BlueprintBodyPart bodyPart, int previous, int current)
	{
		if (EventInvokerExtensions.MechanicEntity != m_MechanicEntity || m_MechanicEntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue)
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
		if (criticalEffectStageBuff2 != null && !TryCreateFromCombatTextsSettings(criticalEffectStageBuff2, CombatTextEventType.OnAttach, isPersonal: false))
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
		if (EventInvokerExtensions.MechanicEntity == m_MechanicEntity)
		{
			BlueprintBuff criticalEffectStageBuff = bodyPart.GetCriticalEffectStageBuff(failedStage);
			if (criticalEffectStageBuff != null)
			{
				m_CombatMessage.Execute(new CombatMessageCriticalEffectCheckFailed
				{
					EffectName = criticalEffectStageBuff.Name
				});
			}
		}
	}

	public void HandleMoraleValueChanged(int delta, bool hasCriticalEffect)
	{
		if (delta != 0 && EventInvokerExtensions.MechanicEntity == m_MechanicEntity && !m_MechanicEntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue)
		{
			m_PendingMoraleDelta += delta;
			m_PendingHasCriticalEffect |= hasCriticalEffect;
			m_MoraleDelayDisposable?.Dispose();
			m_MoraleDelayDisposable = DelayedInvoker.InvokeInFrames(PushMoraleMessage, 10);
		}
	}

	private void PushMoraleMessage()
	{
		CombatMessageMorale parameter = new CombatMessageMorale
		{
			Amount = m_PendingMoraleDelta,
			HasCriticalEffect = m_PendingHasCriticalEffect
		};
		m_CombatMessage.Execute(parameter);
		m_PendingMoraleDelta = 0;
		m_PendingHasCriticalEffect = false;
		m_MoraleDelayDisposable?.Dispose();
		m_MoraleDelayDisposable = null;
	}

	void IMoralePhaseHandler.HandleMoralePhaseChanged(MoralePhaseType phase)
	{
		if (EventInvokerExtensions.MechanicEntity == m_MechanicEntity && !m_MechanicEntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue)
		{
			m_CombatMessage.Execute(new CombatMessageMoralePhase
			{
				PhaseType = phase
			});
		}
	}

	public void HandleAttackOfOpportunity(BaseUnitEntity target)
	{
		m_CombatMessage.Execute(new CombatMessageAttackOfOpportunity());
	}

	public void OnEventAboutToTrigger(RulePerformSavingThrow evt)
	{
	}

	public void OnEventDidTrigger(RulePerformSavingThrow evt)
	{
		if (Rulebook.CurrentContext.Current != null && Rulebook.CurrentContext.Current.Initiator == m_MechanicEntity)
		{
			RuleDealDamage ruleDealDamage = Rulebook.CurrentContext.LastEventOfType<RuleDealDamage>();
			if (evt.IsPassed || ruleDealDamage?.Initiator != evt.Initiator)
			{
				m_CombatMessage.Execute(new CombatMessageSavingThrow
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
		if (Rulebook.CurrentContext.Current.Initiator == m_MechanicEntity && !evt.Ability.Blueprint.DisableLog && !m_MechanicEntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue && !TryCreateFromCombatTextsSettings(evt.Ability?.Blueprint?.OriginalBlueprint, CombatTextEventType.OnAttach, isPersonal: true) && Rulebook.CurrentContext.Current != null)
		{
			m_CombatMessage.Execute(new CombatMessageAbility
			{
				Name = evt.Ability.Blueprint.Name,
				Sprite = evt.Ability.Blueprint.Icon
			});
		}
	}

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	public void HandleAttack(RulePerformAttack withWeaponAttackHit)
	{
		RuleDealDamage resultDamageRule = withWeaponAttackHit.ResultDamageRule;
		if ((resultDamageRule == null || !resultDamageRule.IsFake) && withWeaponAttackHit.Target == m_MechanicEntity)
		{
			if (withWeaponAttackHit.ResultIsHit)
			{
				WarhammerAttackHit(withWeaponAttackHit);
				return;
			}
			m_CombatMessage.Execute(new CombatMessageAttackMiss
			{
				Result = withWeaponAttackHit.Result,
				IsCasterCriticallyInjured = withWeaponAttackHit.RollPerformAttackRule.MissCausedByCritOnSelf,
				SourcePosition = (withWeaponAttackHit.Projectile?.LaunchPosition ?? withWeaponAttackHit.Initiator?.Position ?? Vector3.one),
				TargetPosition = (withWeaponAttackHit.Projectile?.CorePosition ?? withWeaponAttackHit.Target?.Position ?? Vector3.one)
			});
		}
	}

	private void WarhammerAttackHit(RulePerformAttack withWeaponAttackHit)
	{
		RuleDealDamage resultDamageRule = withWeaponAttackHit.ResultDamageRule;
		if (m_MechanicEntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue || resultDamageRule == null || TryCreateFromCombatTextsSettings(withWeaponAttackHit.Ability.Blueprint.BaseAbility, CombatTextEventType.OnAttach, isPersonal: false))
		{
			return;
		}
		Sprite sprite = null;
		if (Rulebook.CurrentContext.First is RulePerformAbility rulePerformAbility)
		{
			sprite = rulePerformAbility.Ability.Blueprint.Icon;
		}
		if (sprite == null && resultDamageRule != null)
		{
			if (resultDamageRule.Reason.Ability != null)
			{
				sprite = resultDamageRule.Reason.Ability.Blueprint.Icon;
			}
			else if (resultDamageRule.Reason.Fact != null)
			{
				BlueprintMechanicEntityFact blueprint = resultDamageRule.Reason.Fact.Blueprint;
				if (!(blueprint is BlueprintAbility blueprintAbility))
				{
					if (!(blueprint is BlueprintActivatableAbility blueprintActivatableAbility))
					{
						if (blueprint is BlueprintBuff blueprintBuff)
						{
							sprite = blueprintBuff.Icon;
						}
					}
					else
					{
						sprite = blueprintActivatableAbility.Icon;
					}
				}
				else
				{
					sprite = blueprintAbility.Icon;
				}
			}
		}
		if (TryGetDamageBonusSprite(resultDamageRule.ResultDamage, out var sprite2, out var hasHpBonusDamage, out var hasArmorBonusDamage))
		{
			sprite = sprite2;
		}
		CombatMessageDamage parameter = new CombatMessageDamage
		{
			Amount = withWeaponAttackHit.ResultDamageValue,
			Sprite = sprite,
			IsVital = resultDamageRule.ResultDamage.IsVitalDamage,
			IsImmune = false,
			HasHPDamageBonus = hasHpBonusDamage,
			HasArmorDamageBonus = hasArmorBonusDamage,
			IsEnemy = ((m_MechanicEntity as UnitEntity)?.Faction.IsPlayerEnemy ?? false),
			SourcePosition = (resultDamageRule.Projectile?.LaunchPosition ?? resultDamageRule.Initiator?.Position ?? Vector3.one),
			TargetPosition = (resultDamageRule.Projectile?.CorePosition ?? resultDamageRule.Target?.Position ?? Vector3.one)
		};
		m_CombatMessage.Execute(parameter);
	}

	private bool TryGetDamageBonusSprite(RolledDamage damage, out Sprite sprite, out bool hasHpBonusDamage, out bool hasArmorBonusDamage)
	{
		sprite = null;
		hasHpBonusDamage = false;
		hasArmorBonusDamage = false;
		int resultDamageToHealthValue = damage.ResultDamageToHealthValue;
		int resultBonusDamageToHealth = damage.ResultBonusDamageToHealth;
		if (resultDamageToHealthValue > 0 && resultBonusDamageToHealth > 0)
		{
			sprite = ConfigRoot.Instance.UIConfig.UIIcons.HPDamageBonus;
			hasHpBonusDamage = true;
			return true;
		}
		int resultDamageToArmorValue = damage.ResultDamageToArmorValue;
		int resultBonusDamageToArmor = damage.ResultBonusDamageToArmor;
		if (resultDamageToArmorValue > 0 && resultBonusDamageToArmor > 0)
		{
			sprite = ConfigRoot.Instance.UIConfig.UIIcons.ArmorDamageBonus;
			hasArmorBonusDamage = true;
			return true;
		}
		return false;
	}

	public void HandleCustomCombatText(LocalizedString text)
	{
		if (EventInvokerExtensions.MechanicEntity == m_MechanicEntity)
		{
			m_CombatMessage.Execute(new CombatMessageCustom
			{
				Text = text.Text
			});
		}
	}

	public void HandleCultAmbushVisibilityChange()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && baseUnitEntity == m_MechanicEntity)
		{
			m_CombatMessage.Execute(new CombatMessageCultAmbush());
		}
	}

	public void HandleEntityGainFact(EntityFact fact)
	{
		if (fact == null || fact.GetSubscribingEntity() != m_MechanicEntity || m_MechanicEntity.IsDeadOrUnconscious)
		{
			return;
		}
		Buff buff = fact as Buff;
		if ((buff == null || (!buff.Hidden && !buff.Blueprint.CriticalEffect)) && (!(fact.Blueprint is BlueprintAbility) || fact.SourceFact != null))
		{
			bool isPersonal = buff?.SourceFact != null && buff.SourceFact.Owner == m_MechanicEntity;
			if (!TryCreateFromCombatTextsSettings(fact.Blueprint as BlueprintMechanicEntityFact, CombatTextEventType.OnAttach, isPersonal) && buff != null)
			{
				CombatMessageBase message = ((buff.Blueprint.Stacking == StackingType.Replace) ? ((CombatMessageBase)new CombatMessageReplacedAbility(buff.Name, buff.Icon)) : ((CombatMessageBase)new CombatMessageAbility
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
		if (fact == null || fact.GetSubscribingEntity() != m_MechanicEntity || m_MechanicEntity.IsDeadOrUnconscious)
		{
			return;
		}
		Buff buff = fact as Buff;
		if (buff == null || (!buff.Hidden && !buff.Blueprint.CriticalEffect))
		{
			bool isPersonal = buff?.SourceFact != null && buff.SourceFact.Owner == m_MechanicEntity;
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

	protected override void OnDispose()
	{
		m_HasActiveCombatMessage.Value = false;
	}

	private void PrepareAbilityMessagesToSend(BlueprintFact fact, CombatMessageBase message)
	{
		m_AbilityMessages.TryGetValue(fact, out var value);
		if (value != null && value.GetType() != message.GetType())
		{
			m_AbilityMessages.Remove(fact);
			return;
		}
		m_AbilityMessages[fact] = message;
		m_AbilityMessagesTask?.Dispose();
		m_AbilityMessagesTask = DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(delegate
		{
			if (m_MechanicEntity.IsDeadOrUnconscious)
			{
				m_AbilityMessages.Clear();
				m_AbilityMessagesTask?.Dispose();
			}
			else
			{
				foreach (var (blueprintFact2, parameter) in m_AbilityMessages)
				{
					if (CheatsCombat.CombatTextDebugEnabled)
					{
						PFLog.UI.Log(m_MechanicEntityUIState.MechanicEntity.Name + ": " + blueprintFact2.name);
					}
					m_CombatMessage.Execute(parameter);
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

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		MechanicEntity mechanicEntity = m_MechanicEntity;
		if (!mechanicEntity.CanAct)
		{
			BlueprintBuff disabledCommonBuff = ConfigRoot.Instance.SystemMechanics.DisabledCommonBuff;
			if (mechanicEntity.Buffs?.GetBuff(disabledCommonBuff) != null)
			{
				m_CombatMessage.Execute(new CombatMessageAbility
				{
					Name = disabledCommonBuff.Name,
					Sprite = disabledCommonBuff.Icon,
					BigSprite = ConfigRoot.Instance.UIConfig.UIIcons.CantAct
				});
			}
		}
	}

	private CombatTextTargetType GetCombatTextTargetType(bool isPersonal)
	{
		if (m_MechanicEntity is MapObjectEntity)
		{
			return CombatTextTargetType.MapObject;
		}
		if (isPersonal)
		{
			return CombatTextTargetType.Personal;
		}
		if (m_MechanicEntityUIState.IsEnemy.CurrentValue)
		{
			return CombatTextTargetType.Enemy;
		}
		if (m_MechanicEntityUIState.IsPlayerFaction.CurrentValue)
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
