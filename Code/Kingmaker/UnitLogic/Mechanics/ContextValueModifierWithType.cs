using System;
using JetBrains.Annotations;
using Kingmaker.RuleSystem.Rules.Modifiers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

[Serializable]
public class ContextValueModifierWithType : ContextValueModifier
{
	[HideInInspector]
	public ModifierType ModifierType;

	protected override ModifierType Type => ModifierType;

	public ContextValueModifierWithType()
	{
	}

	public ContextValueModifierWithType([NotNull] ContextValue other)
		: base(other)
	{
	}

	public ContextValueModifierWithType([NotNull] ContextValueModifier other)
		: base(other)
	{
		Enabled = other.Enabled;
	}
}
