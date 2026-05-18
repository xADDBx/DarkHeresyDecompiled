using Kingmaker.View.Mechanics.Entities;

namespace Kingmaker.Code.View.Visual.CharacterSystem.EquipmentComponents;

public interface IRemoveEquipmentEntityHandler
{
	void HandleEquipmentEntityRemoved(AbstractUnitEntityView view);
}
