using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateAbilityActionPointCost : RulebookEvent
{
	private const int TwoWeaponAdditionalPenaltyCost = 0;

	private readonly AbilityData m_AbilityData;

	private List<int> m_CostOverrides;

	private List<int> m_DefaultCostOverrides;

	private List<int> m_CostIncreases;

	private List<(int decrease, int min)> m_CostDecreases;

	public int Result { get; set; }

	private int UnmodifiedDefaultCost
	{
		get
		{
			if (m_AbilityData.SettingsFromItem != null && Blueprint.SameAbility(m_AbilityData.SettingsFromItem.Ability))
			{
				return m_AbilityData.SettingsFromItem.AP;
			}
			return m_AbilityData.Blueprint.ActionPointCost;
		}
	}

	public BlueprintAbilityWrapper Blueprint => m_AbilityData?.Blueprint;

	[CanBeNull]
	public ItemEntityWeapon Weapon => m_AbilityData?.Weapon;

	public AbilityData AbilityData => m_AbilityData;

	public override AbilityData MaybeAbility => m_AbilityData ?? base.MaybeAbility;

	public void AddCostOverride(int overrideCost)
	{
		m_CostOverrides.Add(overrideCost);
	}

	public void AddDefaultCostOverride(int overrideCost)
	{
		m_DefaultCostOverrides.Add(overrideCost);
	}

	public void AddCostIncrease(int increaseCost)
	{
		m_CostIncreases.Add(increaseCost);
	}

	public void AddCostDecrease(int decreaseCost, int min)
	{
		m_CostDecreases.Add((decreaseCost, min));
	}

	public RuleCalculateAbilityActionPointCost([NotNull] MechanicEntity initiator, AbilityData ability)
		: base(initiator)
	{
		m_CostOverrides = ListPool<int>.Claim();
		m_DefaultCostOverrides = ListPool<int>.Claim();
		m_CostIncreases = ListPool<int>.Claim();
		m_CostDecreases = ListPool<(int, int)>.Claim();
		m_AbilityData = ability;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		int num = ((m_CostOverrides.Count == 0) ? (-1) : m_CostOverrides.Min());
		int num2 = ((m_DefaultCostOverrides.Count == 0) ? UnmodifiedDefaultCost : m_DefaultCostOverrides.Min());
		int num3 = m_CostIncreases.Sum();
		int num4 = 0;
		if (num >= 0)
		{
			num4 = num;
		}
		else
		{
			num4 = num2 + num3;
			foreach (var item3 in m_CostDecreases.OrderBy(((int decrease, int min) dm) => -dm.min))
			{
				int item = item3.decrease;
				int item2 = item3.min;
				num4 += item;
				if (num4 < item2)
				{
					num4 = item2;
				}
				if (num4 < 0)
				{
					num4 = 0;
				}
			}
		}
		Result = (HasPenaltyCost() ? num4 : num4);
		ListPool<int>.Release(m_CostOverrides);
		ListPool<int>.Release(m_DefaultCostOverrides);
		ListPool<int>.Release(m_CostIncreases);
		ListPool<(int, int)>.Release(m_CostDecreases);
		m_CostOverrides = null;
		m_DefaultCostOverrides = null;
		m_CostIncreases = null;
		m_CostDecreases = null;
	}

	private bool HasPenaltyCost()
	{
		PartTwoWeaponFighting twoWeaponFightingOptional = m_AbilityData.Caster.GetTwoWeaponFightingOptional();
		ItemEntityWeapon weapon = m_AbilityData.GetWeaponStats().Weapon;
		if (twoWeaponFightingOptional != null && twoWeaponFightingOptional.EnableAttackWithPairedWeapon && twoWeaponFightingOptional.IsOtherAbilityGroupOnCooldown(m_AbilityData) && weapon != null && !weapon.HoldInTwoHands && !m_AbilityData.IsFreeAction && !m_AbilityData.Caster.Features.HasNoAPPenaltyCostForTwoWeaponFighting && m_AbilityData.Blueprint.Type == AbilityType.Weapon)
		{
			return !m_AbilityData.IsBonusUsage;
		}
		return false;
	}
}
