using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintProjectile))]
[TypeId("57705fa312463904bb06fe41a8afb0f1")]
public class CannotSneakAttack : EntityFactComponentDelegate
{
}
