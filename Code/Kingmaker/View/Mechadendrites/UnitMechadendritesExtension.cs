using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.View.Mechadendrites;

public static class UnitMechadendritesExtension
{
	public static bool HasMechadendrites(this MechanicEntity unit)
	{
		return unit?.GetOptional<UnitPartMechadendrites>() != null;
	}

	public static bool HasMechadendriteOfType(this MechanicEntity unit, MechadendritesType type)
	{
		return (unit?.GetOptional<UnitPartMechadendrites>())?.Mechadendrites.ContainsKey(type) ?? false;
	}
}
