using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("18beb8746b848f6448d3ee6969a32467")]
public class OverrideAbilityApCost : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityActionPointCost>, IRulebookHandler<RuleCalculateAbilityActionPointCost>, ISubscriber, IInitiatorRulebookSubscriber
{
	public enum WeaponAPParameter
	{
		Single,
		Burst,
		Special,
		StaticConstant,
		Reload,
		CostBonus,
		Zero,
		DefaultCost
	}

	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	[SerializeField]
	private WeaponAPParameter m_overrideMode;

	[SerializeField]
	[ShowIf("m_NewCost")]
	private int m_newCost;

	[SerializeField]
	[ShowIf("CostBonus")]
	private int m_costBonus;

	[SerializeField]
	[ShowIf("CostBonus")]
	[InfoBox("Оставьте -1, если переопределять минимальный Cost не нужно")]
	private int m_costMinimum = -1;

	[InfoBox(Text = "Ability или группа, к которым нужно применять изменение стоимости AP. Если оба не заданы, применяется к текущей")]
	[SerializeField]
	[CanBeNull]
	private BlueprintAbilityReference m_Ability;

	[SerializeField]
	private BlueprintAbilityGroupReference m_AffectedGroup;

	public bool NotSelectedGroup;

	public bool AffectOnlyMelee;

	public bool AffectOnlyRanged;

	public bool AffectOnlyAoE;

	public bool AffectOnlyBurst;

	public bool AffectOnlyHeavy;

	public BlueprintAbility Ability => m_Ability?.Get();

	public BlueprintAbilityGroup AffectedGroup => m_AffectedGroup?.Get();

	public bool StaticConst => m_overrideMode == WeaponAPParameter.StaticConstant;

	public bool CostBonus => m_overrideMode == WeaponAPParameter.CostBonus;

	private bool DefaultCost => m_overrideMode == WeaponAPParameter.DefaultCost;

	private bool m_NewCost
	{
		get
		{
			if (!StaticConst)
			{
				return DefaultCost;
			}
			return true;
		}
	}

	public void OnEventAboutToTrigger(RuleCalculateAbilityActionPointCost evt)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Context, null, null, evt))
			{
				return;
			}
		}
		if ((AffectedGroup == null && Ability == null && Restrictions.Property.Empty && !evt.Blueprint.SameAbility(base.OwnerBlueprint as BlueprintAbility)) || (AffectedGroup != null && ((NotSelectedGroup && evt.Blueprint.AbilityGroups.Contains(AffectedGroup)) || (!NotSelectedGroup && !evt.Blueprint.AbilityGroups.Contains(AffectedGroup)))) || (Ability != null && !evt.Blueprint.SameAbility(Ability)))
		{
			return;
		}
		if (AffectOnlyMelee)
		{
			ItemEntityWeapon weapon = evt.Weapon;
			if (weapon == null || !weapon.Blueprint.IsMelee)
			{
				return;
			}
		}
		if (AffectOnlyRanged)
		{
			ItemEntityWeapon weapon2 = evt.Weapon;
			if (weapon2 == null || !weapon2.Blueprint.IsRanged)
			{
				return;
			}
		}
		if ((AffectOnlyAoE && !evt.Blueprint.IsAoE) || (AffectOnlyBurst && !evt.Blueprint.IsBurst))
		{
			return;
		}
		if (AffectOnlyHeavy)
		{
			ItemEntityWeapon weapon3 = evt.Weapon;
			if (weapon3 == null || weapon3.Blueprint.Heaviness != WeaponHeaviness.Heavy)
			{
				return;
			}
		}
		if (CostBonus)
		{
			if (m_costBonus > 0)
			{
				evt.AddCostIncrease(m_costBonus);
			}
			else
			{
				evt.AddCostDecrease(m_costBonus, m_costMinimum);
			}
		}
		if (StaticConst && m_newCost >= 0)
		{
			evt.AddCostOverride(m_newCost);
		}
		if (DefaultCost && m_newCost >= 0)
		{
			evt.AddDefaultCostOverride(m_newCost);
		}
		if (m_overrideMode == WeaponAPParameter.Zero)
		{
			evt.AddCostOverride(0);
		}
	}

	public void OnEventDidTrigger(RuleCalculateAbilityActionPointCost evt)
	{
	}
}
