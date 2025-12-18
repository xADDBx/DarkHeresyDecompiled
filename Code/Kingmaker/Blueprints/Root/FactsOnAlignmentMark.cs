using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.Fmw.Blueprints;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class FactsOnAlignmentMark
{
	[SerializeField]
	public AlignmentAxis Axis;

	[SerializeField]
	public int RankRequired;

	[SerializeField]
	public List<BpRef<BlueprintMechanicEntityFact>> Facts = new List<BpRef<BlueprintMechanicEntityFact>>();
}
