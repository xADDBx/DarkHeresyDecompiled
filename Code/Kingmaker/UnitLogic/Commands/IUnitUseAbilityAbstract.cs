using Kingmaker.Controllers;

namespace Kingmaker.UnitLogic.Commands;

public interface IUnitUseAbilityAbstract
{
	AbilityExecutionProcess ExecutionProcess { get; }
}
