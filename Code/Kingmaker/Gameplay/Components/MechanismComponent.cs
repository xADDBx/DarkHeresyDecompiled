using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[Obsolete("Use ForbidDirectHpDamageComponent, CountHpAsArmorComponent, CountAsDestructibleComponent instead.")]
[ClassInfoBox("OBSOLETE. Use ForbidDirectHpDamageComponent + CountHpAsArmorComponent + CountAsDestructibleComponent. This component is kept for backward compatibility with existing blueprints.")]
[ComponentName("Custom/MechanismComponent")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("d9e7e15f00354f1aa3722407061d696d")]
public sealed class MechanismComponent : UnitFactComponentDelegate
{
	protected override void OnFactAttached()
	{
		base.Owner.GetOrCreate<PartMechanism>().Retain();
		PartHealth required = base.Owner.GetRequired<PartHealth>();
		required.RetainForbidDirectHpDamage();
		required.RetainCountHpAsArmor();
		required.RetainCountAsDestructible();
	}

	protected override void OnFactDetached()
	{
		base.Owner.GetOptional<PartMechanism>()?.Release();
		PartHealth required = base.Owner.GetRequired<PartHealth>();
		required.ReleaseForbidDirectHpDamage();
		required.ReleaseCountHpAsArmor();
		required.ReleaseCountAsDestructible();
	}
}
