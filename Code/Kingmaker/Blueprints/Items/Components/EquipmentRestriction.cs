using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints.Items.Components;

[AllowedOn(typeof(BlueprintItemEquipment))]
[TypeId("7f0e948cbdc624e43b28a69961c69517")]
public abstract class EquipmentRestriction : BlueprintComponent
{
	public abstract bool CanBeEquippedBy(MechanicEntity unit);
}
