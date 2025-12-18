using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("43e0c6e5ae7e0ef498d5bcc59180ee94")]
public class WarhammerLoseCover : UnitFactComponentDelegate
{
	public ContextValue losePartPct;

	public bool onlyFromBuffCaster;
}
