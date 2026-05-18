using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
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
			if (EvalContext.Current.Target?.Entity != target)
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
		using (EvalContext.Current.PushTarget(target))
		{
			actions.Run();
		}
	}

	public static void RunWithTarget(this ActionList actions, [NotNull] MechanicEntity target)
	{
		if (actions.HasActions)
		{
			if (EvalContext.Current.Target?.Entity != target)
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
