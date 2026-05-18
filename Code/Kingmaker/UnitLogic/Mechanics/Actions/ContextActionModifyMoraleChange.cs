using System;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[TypeId("caefbc0ff66749c39f9828333405d667")]
public sealed class ContextActionModifyMoraleChange : ContextAction
{
	public ModifierDescriptor Descriptor;

	public ContextValueModifierWithType Modifier = new ContextValueModifierWithType
	{
		Enabled = true
	};

	public ContextValueModifierWithType PositiveModifier = new ContextValueModifierWithType
	{
		Enabled = true
	};

	public ContextValueModifierWithType NegativeModifier = new ContextValueModifierWithType
	{
		Enabled = true
	};

	public override string GetCaption()
	{
		return $"Apply {Modifier} modifier to morale change";
	}

	protected override void RunAction()
	{
		RuleCalculateMoraleChange ruleCalculateMoraleChange = (base.Context.Rule as RuleCalculateMoraleChange) ?? (base.Context.Rule as RulePerformMoraleChange)?.RuleCalculateMoraleChange;
		if (ruleCalculateMoraleChange == null)
		{
			throw new InvalidOperationException("Cannot find RuleCalculateMoraleChange");
		}
		if (ruleCalculateMoraleChange.IsTriggered)
		{
			throw new InvalidOperationException("Cannot modify RuleCalculateMoraleChang after it is triggered");
		}
		Modifier.TryApply(ruleCalculateMoraleChange.ValueModifier, base.Context.Fact, Descriptor);
		PositiveModifier.TryApply(ruleCalculateMoraleChange.PositiveValueModifier, base.Context.Fact, Descriptor);
		NegativeModifier.TryApply(ruleCalculateMoraleChange.NegativeValueModifier, base.Context.Fact, Descriptor);
	}
}
