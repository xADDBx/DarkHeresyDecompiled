using System;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Encyclopedia;

[Obsolete]
[TypeId("7bdb8039e4f8442fbc541a34e590d8fa")]
public class BlueprintEncyclopediaPlanetTypePage : BlueprintEncyclopediaPage
{
	[SerializeField]
	public BlueprintPlanet.PlanetType PlanetType;

	public bool IsAvailable => false;
}
