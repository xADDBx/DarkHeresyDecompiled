using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[PlayerUpgraderAllowed(false)]
[TypeId("a267f44c55cb45f4982bf54563ffbf2a")]
public class ContextActionChangeRankInFact : ContextAction
{
	[SerializeField]
	private BlueprintUnitFactReference m_Blueprint;

	[SerializeField]
	private bool m_ExistingFactsOnly;

	[SerializeField]
	[ShowIf("m_ExistingFactsOnly")]
	private bool m_FactFromCaster;

	[SerializeField]
	private ContextValue m_Value;

	private BlueprintUnitFact Blueprint => m_Blueprint?.Get();

	public override string GetCaption()
	{
		string arg = (m_ExistingFactsOnly ? "existing " : "");
		return $"Change rank(s) by {m_Value} in {arg} fact feature.";
	}

	protected override void RunAction()
	{
		if (base.Target.Entity == null)
		{
			throw new InvalidOperationException("Invalid target for effect '" + GetType().Name + "'");
		}
		if (!(Blueprint is IBlueprintFactWithRanks))
		{
			throw new InvalidOperationException("Invalid Blueprint type: doesn't have Ranks");
		}
		int num = m_Value.Calculate(base.Context);
		bool flag = false;
		foreach (EntityFact item in base.Target.Entity.Facts.List)
		{
			if (item.Blueprint == Blueprint && (!m_FactFromCaster || item.MaybeContext?.MaybeCaster == base.Context.Caster) && item is IFactWithRanks factWithRanks)
			{
				if (num > 0)
				{
					factWithRanks.AddRank(num);
				}
				if (num < 0)
				{
					factWithRanks.RemoveRank(Math.Abs(num));
				}
				flag = true;
			}
		}
		if (!(m_ExistingFactsOnly || flag) && num >= 1)
		{
			base.Target.Entity.Facts.Add(Blueprint.CreateFact(base.Context, null, num));
		}
	}
}
