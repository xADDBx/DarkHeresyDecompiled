using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ClassInfoBox("Unit is immune to HealthOnly damage strategy and non-ArmorOnly healing.")]
[ComponentName("Custom/ForbidDirectHpDamageComponent")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("fc078437278ced34e978d6b3a2c2dfcf")]
public sealed class ForbidDirectHpDamageComponent : UnitFactComponentDelegate
{
	protected override void OnFactAttached()
	{
		base.Owner.GetRequired<PartHealth>().RetainForbidDirectHpDamage();
	}

	protected override void OnFactDetached()
	{
		base.Owner.GetRequired<PartHealth>().ReleaseForbidDirectHpDamage();
	}
}
