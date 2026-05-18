using System;
using JetBrains.Annotations;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Mechanics.Facts;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

[Serializable]
public class ContextValueModifier : ContextValue
{
	[HideInInspector]
	public bool Enabled;

	protected virtual ModifierType Type => ModifierType.ValAdd;

	public ContextValueModifier()
	{
	}

	public ContextValueModifier([NotNull] ContextValue other)
		: base(other)
	{
	}

	public void TryApply([NotNull] ValueModifiersManager manager, [NotNull] MechanicEntityFact sourceFact, ModifierDescriptor descriptor)
	{
		if (Enabled)
		{
			int num = Calculate(EvalContext.Current);
			if (num != 0)
			{
				manager.Add(num, sourceFact, descriptor);
			}
		}
	}

	public void TryApply([NotNull] CompositeModifiersManager target, [NotNull] MechanicEntityFact sourceFact, ModifierDescriptor descriptor)
	{
		if (Enabled)
		{
			int num = Calculate(EvalContext.Current);
			if (num != 0)
			{
				target.Add(Type, num, sourceFact, descriptor);
			}
		}
	}

	public override string ToString()
	{
		return $"{Type}[{base.ToString()}]";
	}
}
