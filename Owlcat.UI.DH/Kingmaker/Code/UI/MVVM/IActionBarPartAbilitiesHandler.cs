using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Facts;

namespace Kingmaker.Code.UI.MVVM;

public interface IActionBarPartAbilitiesHandler : ISubscriber
{
	void MoveSlot(Ability sourceAbility, int targetIndex);

	void MoveSlot(MechanicActionBarSlot sourceSlot, int sourceIndex, int targetIndex);

	void SetSlot(MechanicEntityFact ability, int targetIndex);

	void DeleteSlot(int sourceIndex);

	void ChooseAbilityToSlot(int targetIndex);

	void SetMoveAbilityMode(bool on);
}
