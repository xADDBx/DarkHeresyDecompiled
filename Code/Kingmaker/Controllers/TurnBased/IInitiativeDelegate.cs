using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Controllers.TurnBased;

public interface IInitiativeDelegate
{
	MechanicEntity Delegate { get; }
}
