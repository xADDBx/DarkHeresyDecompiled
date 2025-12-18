using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[PlayerUpgraderAllowed(false)]
[TypeId("4d115ca35e2e4ff2b95e14860b1def39")]
public class GainColonyProjectReward : GameAction
{
	[SerializeField]
	private BlueprintColonyProjectReference m_Project;

	private BlueprintColonyProject Project => m_Project?.Get();

	public override string GetCaption()
	{
		return "Gain project " + Project.Name + " reward despite its status";
	}

	protected override void RunAction()
	{
	}
}
