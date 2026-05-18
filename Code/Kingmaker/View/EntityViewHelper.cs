using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View.Mechanics;

namespace Kingmaker.View;

public static class EntityViewHelper
{
	public static EntityViewBase GetView(this Entity entity)
	{
		return entity.View as EntityViewBase;
	}

	public static MechanicEntityView GetView(this MechanicEntity entity)
	{
		return entity.View as MechanicEntityView;
	}

	public static UnitEntityView GetView(this BaseUnitEntity entity)
	{
		return entity.View as UnitEntityView;
	}
}
