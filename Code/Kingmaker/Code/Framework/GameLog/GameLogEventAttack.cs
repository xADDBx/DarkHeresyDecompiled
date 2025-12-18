using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Localization;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Enums;

namespace Kingmaker.Code.Framework.GameLog;

public class GameLogEventAttack : GameLogRuleEvent<RulePerformAttack, GameLogEventAttack>
{
	public class WeaponSkillModifierValues
	{
		public readonly int ResultValue;

		private readonly List<string> m_KeyOrders;

		private readonly Dictionary<string, int> m_Values;

		public WeaponSkillModifierValues(int resultValue, int baseValue)
		{
			ResultValue = resultValue;
			m_KeyOrders = new List<string>();
			m_Values = new Dictionary<string, int>();
			LocalizedString baseValue2 = LocalizedTexts.Instance.Descriptors.BaseValue;
			m_KeyOrders.Add(baseValue2);
			m_Values.Add(baseValue2, baseValue);
		}

		public void AddModifier(Modifier modifier)
		{
			if (modifier.Value != 0)
			{
				string text = StatModifiersBreakdown.GetBonusSourceText(modifier);
				if (string.IsNullOrEmpty(text))
				{
					text = LocalizedTexts.Instance.Descriptors.GetText(modifier.Descriptor);
				}
				if (!m_Values.ContainsKey(text))
				{
					m_KeyOrders.Add(text);
					m_Values.Add(text, 0);
				}
				m_Values[text] += modifier.Value;
			}
		}

		public IEnumerable<Tuple<string, int>> GetModifiers()
		{
			foreach (string keyOrder in m_KeyOrders)
			{
				yield return new Tuple<string, int>(keyOrder, m_Values[keyOrder]);
			}
		}
	}

	[CanBeNull]
	private List<GameLogRuleEvent<RuleDealDamage>> m_TargetDamageList;

	private readonly Dictionary<MechanicsFeatureType, List<FeatureCountableFlag.FactsList.Element>> m_AssociatedBuffs = new Dictionary<MechanicsFeatureType, List<FeatureCountableFlag.FactsList.Element>>();

	private readonly List<FeatureCountableFlag.FactsList.Element> m_Empty = new List<FeatureCountableFlag.FactsList.Element>();

	public WeaponSkillModifierValues InitiatorWeaponSkillModifierValues { get; private set; }

	public WeaponSkillModifierValues TargetWeaponSkillModifierValues { get; private set; }

	public MechanicEntity Attacker => base.Rule.ConcreteInitiator;

	public MechanicEntity Target => base.Rule.ConcreteTarget;

	public MechanicEntity PointerTarget => base.Rule.TargetUnit;

	public RulePerformAttackRoll RollPerformAttackRule => base.Rule.RollPerformAttackRule;

	public BlueprintBodyPart TargetBodyPart => base.Rule.ResultHitLocation;

	public List<MechanicEntity> TargetsInPattern => null;

	public RuleRollDamage RollRuleDamage => base.Rule.RuleRollDamage;

	public int TargetDamageValue => m_TargetDamageList?.Sum((GameLogRuleEvent<RuleDealDamage> i) => i.Rule.ResultValue) ?? 0;

	public override bool IsEnabled
	{
		get
		{
			if (base.IsEnabled)
			{
				if (Target is DestructibleEntity)
				{
					return TargetDamageValue != 0;
				}
				return true;
			}
			return false;
		}
	}

	public List<GameLogRuleEvent<RuleDealDamage>> TargetDamageList => m_TargetDamageList;

	public bool IsPushTrigger { get; private set; }

	public int? AssassinLethality { get; }

	public GameLogEventAttack(RulePerformAttack rule)
		: base(rule)
	{
		AssassinLethality = null;
		MechanicEntity concreteInitiator = rule.ConcreteInitiator;
		if (concreteInitiator != null && concreteInitiator.MainFact != null && concreteInitiator.IsPlayerFaction)
		{
			BaseUnitEntity initiatorUnit = rule.InitiatorUnit;
			if (initiatorUnit != null && initiatorUnit.HasAssassinCareer)
			{
				AssassinLethality = ConfigRoot.Instance.SystemMechanics.AssassinLethalityProperty?.GetValue(new PropertyContext(null, rule.ConcreteInitiator.MainFact.Context));
			}
		}
		foreach (Tuple<BaseUnitEntity, List<MechanicsFeatureType>> item2 in new List<Tuple<BaseUnitEntity, List<MechanicsFeatureType>>>
		{
			new Tuple<BaseUnitEntity, List<MechanicsFeatureType>>(base.Rule.InitiatorUnit, new List<MechanicsFeatureType>
			{
				MechanicsFeatureType.AutoHit,
				MechanicsFeatureType.AutoMiss
			}),
			new Tuple<BaseUnitEntity, List<MechanicsFeatureType>>(base.Rule.TargetUnit, new List<MechanicsFeatureType>())
		})
		{
			if (item2.Item1 == null)
			{
				continue;
			}
			List<MechanicsFeatureType> item = item2.Item2;
			if (item == null || item.Count <= 0)
			{
				continue;
			}
			foreach (MechanicsFeatureType item3 in item2.Item2)
			{
				List<FeatureCountableFlag.FactsList.Element> list = item2.Item1.GetMechanicFeature(item3).AssociatedFacts.Elements.ToList();
				if (!m_AssociatedBuffs.ContainsKey(item3))
				{
					m_AssociatedBuffs.Add(item3, new List<FeatureCountableFlag.FactsList.Element>());
				}
				foreach (FeatureCountableFlag.FactsList.Element item4 in list)
				{
					if (!m_AssociatedBuffs[item3].Contains(item4))
					{
						m_AssociatedBuffs[item3].Add(item4);
					}
				}
			}
		}
	}

	private IReadOnlyList<FeatureCountableFlag.FactsList.Element> GetMechanicFeatureAssociatedBuffs(MechanicsFeatureType type)
	{
		if (!m_AssociatedBuffs.TryGetValue(type, out var value))
		{
			return m_Empty;
		}
		return value;
	}

	public IReadOnlyList<FeatureCountableFlag.FactsList.Element> GetAutoHitAssociatedBuffs()
	{
		return GetMechanicFeatureAssociatedBuffs(MechanicsFeatureType.AutoHit);
	}

	public IReadOnlyList<FeatureCountableFlag.FactsList.Element> GetAutoMissAssociatedBuffs()
	{
		return GetMechanicFeatureAssociatedBuffs(MechanicsFeatureType.AutoMiss);
	}

	protected override bool TryHandleInnerEventInternal(GameLogEvent @event)
	{
		if (@event is GameLogRuleEvent<RuleDealDamage> gameLogRuleEvent && gameLogRuleEvent.Rule.Target == Target)
		{
			if (m_TargetDamageList == null)
			{
				m_TargetDamageList = new List<GameLogRuleEvent<RuleDealDamage>>();
			}
			if (!m_TargetDamageList.Contains(gameLogRuleEvent))
			{
				m_TargetDamageList.Add(gameLogRuleEvent);
			}
		}
		if (@event is GameLogRuleEvent<RulePerformSkillCheck>)
		{
			return false;
		}
		return base.TryHandleInnerEventInternal(@event);
	}
}
