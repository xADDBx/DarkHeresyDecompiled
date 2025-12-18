using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[Obsolete("Use ModifyChanceRoll instead")]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintBuff))]
[AllowMultipleComponents]
[TypeId("735b0f7df0d179c4485cd3c8e30ca6db")]
public class RerollD100 : MechanicEntityFactComponentDelegate, IGlobalRulebookHandler<RuleRollChance>, IRulebookHandler<RuleRollChance>, ISubscriber, IGlobalRulebookSubscriber, IGlobalRulebookHandler<RulePerformAttackRoll>, IRulebookHandler<RulePerformAttackRoll>
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public bool ShouldDoActionsOnReroll = true;

		public bool ShouldRemoveSelfAfterReroll;
	}

	[SerializeField]
	private PropertyCalculator m_Chance = new PropertyCalculator();

	[SerializeField]
	private bool m_CanBeGreaterThanBaseChance;

	[SerializeField]
	private bool m_ShouldStopAfterSuccessfulReroll;

	[SerializeField]
	[EnumFlagsAsButtons]
	private ChanceRollType m_RerollSuccess;

	[SerializeField]
	[EnumFlagsAsButtons]
	private ChanceRollType m_RerollFail;

	[SerializeField]
	private bool m_RerollOnlyAgainstSourceOwner;

	[SerializeField]
	[ShowIf("RollTypeIsAttribute")]
	private bool m_RerollConcreteAttribute;

	[SerializeField]
	[ShowIf("RollTypeIsSkill")]
	private bool m_RerollConcreteSkill;

	[SerializeField]
	private bool m_AllowRerollMultipleTimes;

	[SerializeField]
	[ShowIf("AllowRerollMultipleTimes")]
	private PropertyCalculator m_RerollCount = new PropertyCalculator();

	[ShowIf("CanRerollConcreteSkill")]
	public SkillType SkillType;

	[ShowIf("CanRerollConcreteAttribute")]
	public AttributeType AttributeType;

	public bool ModifyBaseChance;

	public bool RemoveSelfAfterReroll;

	public ActionList ActionsOnReroll;

	[UsedImplicitly]
	private bool RollTypeIsSkill
	{
		get
		{
			if ((m_RerollFail & ChanceRollType.Skill) == 0)
			{
				return (m_RerollSuccess & ChanceRollType.Skill) != 0;
			}
			return true;
		}
	}

	[UsedImplicitly]
	private bool RollTypeIsAttribute
	{
		get
		{
			if ((m_RerollFail & ChanceRollType.Attribute) == 0)
			{
				return (m_RerollSuccess & ChanceRollType.Attribute) != 0;
			}
			return true;
		}
	}

	private bool CanRerollConcreteAttribute
	{
		get
		{
			if (m_RerollConcreteAttribute)
			{
				return RollTypeIsAttribute;
			}
			return false;
		}
	}

	private bool CanRerollConcreteSkill
	{
		get
		{
			if (m_RerollConcreteSkill)
			{
				return RollTypeIsSkill;
			}
			return false;
		}
	}

	private bool AllowRerollMultipleTimes => m_AllowRerollMultipleTimes;

	private bool ShouldStopAfterSuccessfulReroll => m_ShouldStopAfterSuccessfulReroll;

	public void OnEventAboutToTrigger(RuleRollChance evt)
	{
		if (!IsEventCorrect(evt))
		{
			return;
		}
		if (!ShouldStopAfterSuccessfulReroll)
		{
			RequestTransientData<ComponentData>().ShouldDoActionsOnReroll = true;
		}
		int num = m_Chance.GetValue(evt.ConcreteInitiator, null, null, evt);
		if (ModifyBaseChance)
		{
			num = evt.OriginalSuccessChance + num;
		}
		if (!m_CanBeGreaterThanBaseChance)
		{
			num = Math.Min(num, evt.OriginalSuccessChance);
		}
		if (num <= 0)
		{
			return;
		}
		int num2 = ((!m_AllowRerollMultipleTimes) ? 1 : m_RerollCount.GetValue(evt.ConcreteInitiator, null, null, evt));
		if (num2 > 0)
		{
			if ((m_RerollSuccess & evt.Type) != 0)
			{
				evt.AddRerollSuccess(num, num2, base.Fact);
			}
			if ((m_RerollFail & evt.Type) != 0)
			{
				evt.AddRerollFail(num, num2, base.Fact);
			}
			if (RemoveSelfAfterReroll)
			{
				RequestTransientData<ComponentData>().ShouldRemoveSelfAfterReroll = true;
			}
		}
	}

	public void OnEventDidTrigger(RuleRollChance evt)
	{
		if (!IsEventCorrect(evt) || evt.OriginalSuccessChance <= 0)
		{
			return;
		}
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (componentData.ShouldDoActionsOnReroll)
		{
			ActionList actionsOnReroll = ActionsOnReroll;
			if (actionsOnReroll != null && actionsOnReroll.HasActions && evt.RollHistory.Count > 1)
			{
				base.Fact.RunActionInContext(ActionsOnReroll, base.Owner.ToITargetWrapper());
				componentData.ShouldDoActionsOnReroll = ((m_RerollSuccess & evt.Type) == 0 || evt.Success) && ((m_RerollFail & evt.Type) == 0 || !evt.Success);
			}
		}
		if (componentData.ShouldRemoveSelfAfterReroll)
		{
			base.Fact.Detach();
		}
	}

	private bool IsEventCorrect(RuleRollChance evt)
	{
		if (evt.Initiator != base.Owner)
		{
			return false;
		}
		if (m_RerollOnlyAgainstSourceOwner && base.Fact.Sources.All((EntityFactSource x) => x.Fact?.Owner != evt.AttackInitiator))
		{
			return false;
		}
		if (evt.IsResultOverriden)
		{
			return false;
		}
		if (RollTypeIsAttribute && evt.AttributeType != AttributeType && (evt.Type & ChanceRollType.Attribute) != 0 && m_RerollConcreteAttribute)
		{
			return false;
		}
		if (RollTypeIsSkill && evt.SkillType != SkillType && (evt.Type & ChanceRollType.Skill) != 0 && m_RerollConcreteSkill)
		{
			return false;
		}
		return true;
	}

	public void OnEventAboutToTrigger(RulePerformAttackRoll evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAttackRoll evt)
	{
		RequestTransientData<ComponentData>().ShouldDoActionsOnReroll = true;
	}
}
