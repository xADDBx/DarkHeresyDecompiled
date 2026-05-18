using System;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class ExplorationConfig
{
	[field: SerializeField]
	public float FlashlightDelayTimer { get; private set; } = 1f;


	[field: SerializeField]
	public float FlashlightNearViewportRadius { get; private set; } = 0.1f;

}
