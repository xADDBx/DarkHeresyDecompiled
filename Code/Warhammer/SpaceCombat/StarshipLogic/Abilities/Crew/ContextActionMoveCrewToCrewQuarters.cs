using System;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities.Crew;

[Obsolete]
[TypeId("08e46dfc93144bd8ac752e649775e5b9")]
public class ContextActionMoveCrewToCrewQuarters : ContextAction
{
	[SerializeField]
	private float m_PercentToMove = 0.3f;

	[SerializeField]
	private ShipModuleType m_ModuleType;

	public override string GetCaption()
	{
		return $"Move {m_PercentToMove} % from module {m_ModuleType} to crew quarters and distribute";
	}

	protected override void RunAction()
	{
	}
}
