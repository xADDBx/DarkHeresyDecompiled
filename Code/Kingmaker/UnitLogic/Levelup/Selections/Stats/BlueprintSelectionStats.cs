using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Selections.Stats;

[Serializable]
[TypeId("9e14ebe6ad644cf9a7083ac75316c469")]
public abstract class BlueprintSelectionStats : BlueprintSelection
{
	[Tooltip("Сколько раз нужно прокачать статы")]
	public int PointsTotal;

	[Tooltip("Сколько раз можно прокачать один стат")]
	public int MaxPointsPerStat;

	public abstract IEnumerable<BlueprintStatAdvancement> Advancements { get; }
}
