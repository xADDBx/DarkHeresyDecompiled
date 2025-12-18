using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Enums;
using Kingmaker.Globalmap.Blueprints.CombatRandomEncounters;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("6c0221930c164574eabd26acbc7cc934")]
public class AddTags : BlueprintComponent
{
	public UnitRole RandomEncounterRole;

	public bool IsRanged;

	public bool IsCaster;

	[ValidateNotNull]
	public UnitTag[] Tags = new UnitTag[0];
}
