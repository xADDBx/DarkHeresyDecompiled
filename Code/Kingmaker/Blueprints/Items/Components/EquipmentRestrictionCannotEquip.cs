using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints.Items.Components;

[Obsolete]
[AllowMultipleComponents]
[TypeId("2f400850d0af2c24d834e4fd682aa91c")]
public class EquipmentRestrictionCannotEquip : EquipmentRestriction
{
	public override bool CanBeEquippedBy(MechanicEntity unit)
	{
		return false;
	}
}
