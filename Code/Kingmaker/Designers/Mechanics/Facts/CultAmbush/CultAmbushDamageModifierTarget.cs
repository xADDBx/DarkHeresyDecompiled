using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Damage;
using Kingmaker.Framework.Mechanics.Actor;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.CultAmbush;

[Obsolete]
[AllowMultipleComponents]
[TypeId("b952465c104f41e6802093ba4366ec40")]
public class CultAmbushDamageModifierTarget : DamageModifier
{
	protected override StatModifierScope Scope => StatModifierScope.Owner;
}
