using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("7d521ff336efba541a0cfa902d15a623")]
public class StarshipAIAbilityRestrictionLogic : BlueprintComponent
{
	public enum LogicMode
	{
		RestartShields,
		StareToAbyss,
		PlasmaDrivesKamikaze,
		DisableDoubleReinforce,
		PlayerStarshipOnly
	}

	public LogicMode logicMode;

	[ShowIf("IsKamikaze")]
	public int lowHP_Percent;

	[ShowIf("IsKamikaze")]
	public int highHP_Percent;

	[ShowIf("IsKamikaze")]
	public int lowHP_Chances;

	[ShowIf("IsKamikaze")]
	public int highHP_Chances;

	[ShowIf("IsReinforceSameSide")]
	public StarshipSectorShieldsType shieldsSector;

	public bool IsKamikaze => logicMode == LogicMode.PlasmaDrivesKamikaze;

	public bool IsReinforceSameSide => logicMode == LogicMode.DisableDoubleReinforce;
}
