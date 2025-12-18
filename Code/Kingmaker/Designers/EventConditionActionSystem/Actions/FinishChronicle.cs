using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("727d31d013cf5d34eac20031d87ee75c")]
[PlayerUpgraderAllowed(false)]
public class FinishChronicle : GameAction
{
	[SerializeField]
	private BlueprintColonyChronicle.Reference m_Chronicle;

	[SerializeField]
	private BlueprintColonyReference m_ColonyBlueprint;

	private BlueprintColonyChronicle Chronicle => m_Chronicle?.Get();

	private BlueprintColony ColonyBlueprint => m_ColonyBlueprint?.Get();

	public override string GetCaption()
	{
		return "Add chronicle to log";
	}

	protected override void RunAction()
	{
	}
}
