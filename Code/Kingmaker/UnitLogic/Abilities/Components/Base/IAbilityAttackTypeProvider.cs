namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityAttackTypeProvider
{
	bool IsMelee { get; }

	bool IsRanged { get; }

	bool IsThrow { get; }

	bool IsSingle { get; }

	bool IsBurst { get; }

	bool IsAoe { get; }

	bool IsPrecise { get; }
}
