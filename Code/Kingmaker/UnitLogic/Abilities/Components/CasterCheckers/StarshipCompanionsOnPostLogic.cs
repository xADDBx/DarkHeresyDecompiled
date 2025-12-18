using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[Obsolete]
[AllowedOn(typeof(BlueprintFeature))]
[TypeId("182075e83588f23428cf054eb5f3668f")]
public class StarshipCompanionsOnPostLogic : BlueprintComponent
{
	public int SkillValueToPassBasicCheck;

	public int MicroabilityCooldownWhenNotPassed;

	public int StartingUltimateRounds;

	public int SkillPointsToAddExtraUltimateRound;
}
