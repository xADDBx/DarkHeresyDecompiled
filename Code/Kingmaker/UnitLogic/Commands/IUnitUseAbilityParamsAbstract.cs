using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.UnitLogic.Commands;

public interface IUnitUseAbilityParamsAbstract
{
	bool DisableLog { get; }

	AttackHitPolicyType HitPolicy { get; }

	DamagePolicyType DamagePolicy { get; }

	bool KillTarget { get; }

	bool IgnoreAbilityUsingInThreateningArea { get; }

	MechanicsContext ParentContext { get; }

	AbilityData Ability { get; }

	bool DisableCameraFollow { get; }

	IAbilityCustomAnimation CustomAnimationOverride { get; }

	bool IgnoreCooldown { get; }

	bool IsDirectionCorrect { get; }
}
