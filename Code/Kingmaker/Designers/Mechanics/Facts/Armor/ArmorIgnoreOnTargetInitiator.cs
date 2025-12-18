using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.Armor;

[Obsolete]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("b2e83cd85780453f854ebe9dac137c42")]
public class ArmorIgnoreOnTargetInitiator : MechanicEntityFactComponentDelegate
{
}
