using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Globalmap.Blueprints.Colonization;

[Obsolete]
[AllowedOn(typeof(BlueprintPlanet))]
[TypeId("1ff28703109a4697bd8e35b30aeda286")]
public class ColonyComponent : BlueprintComponent
{
	public BlueprintColony.Reference ColonyBlueprint;
}
