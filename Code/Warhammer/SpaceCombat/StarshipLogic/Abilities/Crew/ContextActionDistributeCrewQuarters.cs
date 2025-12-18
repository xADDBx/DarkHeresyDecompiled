using System;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities.Crew;

[Obsolete]
[TypeId("2ac906dbac5b4db8a0eb7115e433506b")]
public class ContextActionDistributeCrewQuarters : ContextAction
{
	[SerializeField]
	private int m_DistributeCount;

	public override string GetCaption()
	{
		return $"Distribute {m_DistributeCount} from crew quarters to modules";
	}

	protected override void RunAction()
	{
	}
}
