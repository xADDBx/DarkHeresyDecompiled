using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.PubSubSystem;

public interface IAbilityTargetPossibilityCheck : ISubscriber
{
	void HandleAbilityTargetPossibilityCheck(AbilityData ability, TargetWrapper target, Vector3? pointerPosition, bool targetingIsPossible);
}
