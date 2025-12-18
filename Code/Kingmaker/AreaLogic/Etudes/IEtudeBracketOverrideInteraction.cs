using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.AreaLogic.Etudes;

public interface IEtudeBracketOverrideInteraction
{
	int Distance { get; }

	bool IsDialog { get; }

	bool AllowInCombat { get; }

	AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target);
}
