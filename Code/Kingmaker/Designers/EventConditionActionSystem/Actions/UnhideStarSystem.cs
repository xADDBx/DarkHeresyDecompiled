using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("21478aefd0db492089f79ea2cf10f07b")]
[PlayerUpgraderAllowed(false)]
public class UnhideStarSystem : GameAction
{
	[SerializeField]
	private BlueprintSectorMapPointStarSystem.Reference m_SectorMapPoint;

	[SerializeField]
	private bool m_ExploreSystem = true;

	private BlueprintSectorMapPointStarSystem SectorMapPoint => m_SectorMapPoint?.Get();

	public override string GetCaption()
	{
		return "Unhide " + SectorMapPoint.name;
	}

	protected override void RunAction()
	{
	}
}
