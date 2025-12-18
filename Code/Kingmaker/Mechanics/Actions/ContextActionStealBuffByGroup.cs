using System;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[Obsolete]
[TypeId("9a04ed17510fb25428bd52b683f91e5c")]
public class ContextActionStealBuffByGroup : ContextAction
{
	[SerializeField]
	private BlueprintAbilityGroupReference[] m_Groups;

	[SerializeField]
	private bool m_CopyNotRemove;

	public override string GetCaption()
	{
		if (m_CopyNotRemove)
		{
			return "Duplicates group buffs to itself without removing them from the target";
		}
		return "Duplicates group buffs to itself and removing them from the target";
	}

	protected override void RunAction()
	{
	}
}
