using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.Framework.Mechanics.Actor;

public class StatQueryOutput
{
	private sealed class FullModifiersManager : CompositeModifiersManager
	{
		protected override bool KeepNonStackingModifiers => true;
	}

	public CompositeModifiersManager Modifiers { get; } = new CompositeModifiersManager();


	public CompositeModifiersManager AllModifiers { get; } = new FullModifiersManager();


	public List<StatOverrideEntry> FullOverrides { get; } = new List<StatOverrideEntry>();


	public List<StatOverrideEntry> BaseOverrides { get; } = new List<StatOverrideEntry>();


	public bool HasNonPermanentBonuses
	{
		get
		{
			foreach (Modifier item in Modifiers.List)
			{
				if (!item.Permanent && item.Value > 0)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool HasNonPermanentPenalties
	{
		get
		{
			foreach (Modifier item in Modifiers.List)
			{
				if (!item.Permanent && item.Value < 0)
				{
					return true;
				}
			}
			return false;
		}
	}

	public void Clear()
	{
		Modifiers.Clear();
		AllModifiers.Clear();
		FullOverrides.Clear();
		BaseOverrides.Clear();
	}

	public void CopyFrom(StatModifierCollector collector)
	{
		Modifiers.CopyFrom(collector.Modifiers);
		AllModifiers.CopyFrom(collector.Modifiers);
		FullOverrides.Clear();
		FullOverrides.AddRange(collector.FullOverrides);
		BaseOverrides.Clear();
		BaseOverrides.AddRange(collector.BaseOverrides);
	}
}
