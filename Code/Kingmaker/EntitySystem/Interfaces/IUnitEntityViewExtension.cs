using JetBrains.Annotations;
using Kingmaker.View;

namespace Kingmaker.EntitySystem.Interfaces;

public static class IUnitEntityViewExtension
{
	[CanBeNull]
	public static UnitEntityView AsUnitEntityView([CanBeNull] this IUnitEntityView view)
	{
		return view as UnitEntityView;
	}
}
