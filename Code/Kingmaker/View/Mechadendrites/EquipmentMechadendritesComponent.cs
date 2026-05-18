using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.View.Visual.CharacterSystem.EquipmentComponents;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.View.Mechadendrites;

[AllowedOn(typeof(BlueprintEquipmentFeature))]
[TypeId("359e9bef44534223b2729687c2288252")]
public class EquipmentMechadendritesComponent : BlueprintComponent, IAddEquipmentEntityHandler, IRemoveEquipmentEntityHandler
{
	public void HandleEquipmentEntityAdded(AbstractUnitEntityView view)
	{
		if (!(view == null) && view.Data != null)
		{
			UnitPartMechadendrites orCreate = view.Data.GetOrCreate<UnitPartMechadendrites>();
			MechadendriteSettings[] componentsInChildren = view.GetComponentsInChildren<MechadendriteSettings>();
			foreach (MechadendriteSettings settings in componentsInChildren)
			{
				orCreate.RegisterMechadendrite(settings);
			}
			UpdateBallisticMechadendriteAvailability(view);
		}
	}

	public void HandleEquipmentEntityRemoved(AbstractUnitEntityView view)
	{
		if (!(view == null) && view.Data != null)
		{
			view.Data.GetOptional<UnitPartMechadendrites>()?.UnregisterAllMechadendrites();
			UpdateBallisticMechadendriteAvailability(view);
		}
	}

	private static void UpdateBallisticMechadendriteAvailability(AbstractUnitEntityView view)
	{
		if (view != null && view.AnimationManager != null)
		{
			view.AnimationManager.UpdateBallisticMechadendriteAvailability();
		}
	}
}
