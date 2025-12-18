using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
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
			int value = Calculate(sourceFact.MaybeContext);
			manager.Add(value, sourceFact, descriptor);
		}
	}

	public void TryApply([NotNull] CompositeModifiersManager target, [NotNull] MechanicEntityFact sourceFact, ModifierDescriptor descriptor)
	{
		if (Enabled)
		{
			int value = Calculate(sourceFact.MaybeContext);
			target.Add(Type, value, sourceFact, descriptor);
		}
	}

	public void TryApply([NotNull] ModifiableValue target, [NotNull] EntityFactComponent source, ModifierDescriptor descriptor)
	{
		if (Enabled)
		{
			int value = Calculate(source.Fact.MaybeContext);
			target.AddModifier(Type, value, source, descriptor);
		}
	}

	public override string ToString()
	{
		return $"{Type}[{base.ToString()}]";
	}
}
