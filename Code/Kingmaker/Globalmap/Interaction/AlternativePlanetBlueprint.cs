using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Interaction;

[Obsolete]
[AllowedOn(typeof(BlueprintPlanet))]
[TypeId("e7db42e2087b4848be05526506ffc70e")]
public class AlternativePlanetBlueprint : BlueprintComponent
{
	[SerializeField]
	private BlueprintPlanet.Reference m_Planet;

	public BlueprintPlanet Planet => m_Planet.Get();
}
