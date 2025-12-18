using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;

namespace Kingmaker.PubSubSystem;

public interface IAbilityRangeManualUIHandler : ISubscriber
{
	void HandleSetRangeToCasterPositionManual(AbilityData ability, TargetWrapper targetWrapper);
}
