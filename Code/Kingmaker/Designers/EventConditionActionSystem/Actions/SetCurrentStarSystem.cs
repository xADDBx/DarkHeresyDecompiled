using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("3864bbd5febf4338a505e91fecbeb187")]
public class SetCurrentStarSystem : GameAction
{
	[SerializeField]
	private BlueprintStarSystemMap.Reference m_StarSystem;

	public BlueprintStarSystemMap StarSystem => m_StarSystem?.Get();

	public override string GetCaption()
	{
		return "Set " + StarSystem?.Name + " as current star system";
	}

	protected override void RunAction()
	{
	}
}
