using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("4d7f63e0f8854d8496308bdc0a5e6083")]
public class WarhammerConcentrationActionPoints : UnitFactComponentDelegate
{
	public int ConcentrationActionPointsBonus;
}
