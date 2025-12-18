using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("7def417596434663b95228f37bc18af3")]
[PlayerUpgraderAllowed(false)]
public class ChangePlanetVisual : GameAction
{
	[SerializeField]
	private BlueprintPlanet.Reference m_Planet;

	[SerializeField]
	private BlueprintPlanetPrefab.Reference m_NewVisualBlueprint;

	private BlueprintPlanet Planet => m_Planet?.Get();

	public override string GetCaption()
	{
		return "Change planet visual";
	}

	protected override void RunAction()
	{
	}
}
