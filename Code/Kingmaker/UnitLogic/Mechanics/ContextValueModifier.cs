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

	protected bool IsAdditiveModifier
	{
		get
		{
			if (Type != 0 && Type != ModifierType.ValAdd_Extra)
			{
				return Type == ModifierType.PctAdd;
			}
			return true;
		}
	}

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
			if (num != 0 || !IsAdditiveModifier)
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
			if (num != 0 || !IsAdditiveModifier)
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
