using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat.StarshipLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintStarship))]
[TypeId("6ff23ca998c00ba4aada7019a7ca754a")]
public class StarshipCrewPenalties : BlueprintComponent
{
	public int CrewDamagePerRoundThreshold;

	public ActionList Penalties;
}
