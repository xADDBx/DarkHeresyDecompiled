using JetBrains.Annotations;
using Kingmaker.View;

namespace Kingmaker.EntitySystem.Interfaces;

public static class IEntityViewExtension
{
	[CanBeNull]
	public static EntityViewBase AsEntityView([CanBeNull] this IEntityView view)
	{
		return view as EntityViewBase;
	}
}
