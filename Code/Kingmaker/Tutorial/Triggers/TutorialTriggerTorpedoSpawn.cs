using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[TypeId("79fda134647d40bdbf0a98145b65e379")]
public class TutorialTriggerTorpedoSpawn : BlueprintComponent
{
	private enum Faction
	{
		IsPlayer,
		IsPlayerEnemy
	}

	[SerializeField]
	private Faction m_TorpedoCasterFaction;
}
