using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;

namespace Kingmaker.Framework.Mechanics;

public static class ContextActionExtensions
{
	public static void RunWithTargetIfNotNull(this ActionList actions, [CanBeNull] TargetWrapper target)
	{
		if (actions.HasActions && !(target == null))
		{
			actions.RunWithTarget(target);
		}
	}

	public static void RunWithTargetIfNotNull(this ActionList actions, [CanBeNull] MechanicEntity target)
	{
		if (actions.HasActions && target != null)
		{
			if (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Current?.Entity != target)
			{
				actions.RunWithTarget((TargetWrapper)target);
			}
			else
			{
				actions.Run();
			}
		}
	}

	public static void RunWithTarget(this ActionList actions, [NotNull] TargetWrapper target)
	{
		if (!actions.HasActions)
		{
			return;
		}
		using (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Set(target))
		{
			actions.Run();
		}
	}

	public static void RunWithTarget(this ActionList actions, [NotNull] MechanicEntity target)
	{
		if (actions.HasActions)
		{
			if (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Current?.Entity != target)
			{
				actions.RunWithTarget((TargetWrapper)target);
			}
			else
			{
				actions.Run();
			}
		}
	}
}
