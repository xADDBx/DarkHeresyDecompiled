using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ClassInfoBox("Abilities with CanTargetDestructibleObjects can also target this unit. Standard abilities without that flag can target it normally as well.")]
[ComponentName("Custom/CountAsDestructibleComponent")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("50a24bd168223bf4e8e43656fea1177c")]
public sealed class CountAsDestructibleComponent : UnitFactComponentDelegate
{
	protected override void OnFactAttached()
	{
		base.Owner.GetRequired<PartHealth>().RetainCountAsDestructible();
	}

	protected override void OnFactDetached()
	{
		base.Owner.GetRequired<PartHealth>().ReleaseCountAsDestructible();
	}
}
