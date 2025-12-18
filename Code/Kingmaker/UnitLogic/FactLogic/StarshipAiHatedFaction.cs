using System;
using Code.GameCore.Blueprints;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("f317fa10ff77b3f45b4ac4fa3b99d0e5")]
public class StarshipAiHatedFaction : BlueprintComponent
{
	[SerializeField]
	private BlueprintFactionReference m_HatedFaction;

	public BlueprintFaction HatedFaction => m_HatedFaction?.Get();
}
