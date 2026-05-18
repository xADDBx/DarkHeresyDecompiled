using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.UnitLogic.Interaction;

public interface IUnitInteraction
{
	int Distance { get; }

	int ActionCost { get; }

	bool IsApproach { get; }

	float ApproachCooldown { get; }

	bool MainPlayerPreferred { get; }

	bool IsDialog { get; }

	bool AllowInCombat { get; }

	bool AllowWithHelpless { get; }

	LocalizedString DisplayName { get; }

	bool IsAvailable(BaseUnitEntity initiator, AbstractUnitEntity target);

	AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target);
}
