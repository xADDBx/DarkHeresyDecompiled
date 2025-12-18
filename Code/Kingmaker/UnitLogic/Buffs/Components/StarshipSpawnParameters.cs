using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Serializable]
[Obsolete]
[TypeId("c21ad509e2ca4243bfbd4a07696d8000")]
public class StarshipSpawnParameters : BlueprintComponent
{
	[Tooltip("How much time to wait for spawn VFX to play before starting turn (in seconds).")]
	public float SpawnWaitDuration;
}
