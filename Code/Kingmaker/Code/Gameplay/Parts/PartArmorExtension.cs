using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Code.Gameplay.Parts;

public static class PartArmorExtension
{
	[CanBeNull]
	public static PartArmor GetArmorOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartArmor>();
	}

	[CanBeNull]
	public static PartArmor GetArmorOptional(this IMechanicEntity entity)
	{
		return ((MechanicEntity)entity).GetArmorOptional();
	}
}
