using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("83b515e412962c240830984dd31893e5")]
public class WarhammerContextActionSpawnChildStarship : ContextAction
{
	[SerializeField]
	[FormerlySerializedAs("Blueprint")]
	private BlueprintStarship.Reference m_Blueprint;

	public ActionList AfterSpawn;

	[SerializeField]
	private bool ActBeforeSummoner = true;

	public BlueprintStarship Blueprint => m_Blueprint?.Get();

	public override string GetCaption()
	{
		return "Spawn " + Blueprint.name;
	}

	protected override void RunAction()
	{
	}
}
