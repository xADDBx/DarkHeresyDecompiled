using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("f059179ac7a24674186a7c001b912d2c")]
public class AbilityCustomStarshipNPCTorpedoLaunch : BlueprintComponent
{
	[SerializeField]
	private bool DoubleSpawnMode;

	[SerializeField]
	[HideIf("DoubleSpawnMode")]
	private int m_SpawnRotateLimit;

	[SerializeField]
	private BlueprintStarship.Reference m_TorpedoBlueprint;

	[SerializeField]
	private ActionList m_ActionsOnTorpedo;

	[SerializeField]
	private ActionList m_ActionsOnSelf;

	public BlueprintStarship TorpedoBlueprint => m_TorpedoBlueprint?.Get();
}
