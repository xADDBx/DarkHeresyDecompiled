using System;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.Blueprints.Items;

[Serializable]
public class StatFactionModifierConfig
{
	public StatType Stat;

	public ModifierType ModifierType;

	public int Value;

	public ModifierDescriptor Descriptor;
}
