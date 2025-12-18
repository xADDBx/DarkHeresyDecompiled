using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete("Unused")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("6db23c586b23455a9dadddc032b67a83")]
public class Strangulate : UnitBuffComponentDelegate
{
}
