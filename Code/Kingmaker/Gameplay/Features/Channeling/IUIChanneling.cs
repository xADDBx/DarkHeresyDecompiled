using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;

namespace Kingmaker.Gameplay.Features.Channeling;

public interface IUIChanneling
{
	AbilityData Ability { get; }

	TargetWrapper Target { get; }

	MechanicEntity Owner { get; }

	bool IsActive { get; }
}
