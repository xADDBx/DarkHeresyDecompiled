using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Components.SkillCheck;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("73fa51d8ccd948b684a767a9de627fe7")]
public abstract class SkillCheckModifier : MechanicEntityFactComponentDelegate
{
	public enum ForceResultType
	{
		None,
		Success,
		Failure
	}

	public SkillCheckRestrictionCalculator Restrictions = new SkillCheckRestrictionCalculator();

	public ModifierDescriptor Descriptor;

	public ForceResultType ForceResult;

	[HideIf("IsForceResult")]
	public ContextValueModifierWithType DifficultyModifier = new ContextValueModifierWithType
	{
		Enabled = true
	};

	private bool IsForceResult => ForceResult != ForceResultType.None;

	protected void TryApply(RulePerformSkillCheck rule)
	{
		if (Restrictions.IsPassed(base.Context, null, null, rule))
		{
			switch (ForceResult)
			{
			case ForceResultType.None:
				DifficultyModifier.TryApply(rule.DifficultyModifiers, base.Fact, Descriptor);
				break;
			case ForceResultType.Success:
				rule.ForceResultModifiers.Add(1, base.Fact, Descriptor);
				break;
			case ForceResultType.Failure:
				rule.ForceResultModifiers.Add(-1, base.Fact, Descriptor);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}
}
