using JetBrains.Annotations;
using Kingmaker.View.Mechanics.Entities;

namespace Kingmaker.EntitySystem.Interfaces;

public static class IAbstractUnitEntityViewExtension
{
	[CanBeNull]
	public static AbstractUnitEntityView AsAbstractUnitEntityView([CanBeNull] this IAbstractUnitEntityView view)
	{
		return view as AbstractUnitEntityView;
	}
}
