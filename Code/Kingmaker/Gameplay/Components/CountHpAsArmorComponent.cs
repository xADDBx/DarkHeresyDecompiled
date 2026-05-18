using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ClassInfoBox("Unit's HP counts as armor. ArmorDurability equals HitPoints. Vital attacks are disabled, armor damage bonuses always apply. On activation HP damage becomes armor damage; on deactivation damage splits back (armor first, remainder to HP).")]
[ComponentName("Custom/CountHpAsArmorComponent")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("ab4dc6d36b460ee4588cad9b0ad3ae3e")]
public sealed class CountHpAsArmorComponent : UnitFactComponentDelegate
{
	protected override void OnFactAttached()
	{
		base.Owner.GetRequired<PartHealth>().RetainCountHpAsArmor();
	}

	protected override void OnFactDetached()
	{
		base.Owner.GetRequired<PartHealth>().ReleaseCountHpAsArmor();
	}
}
