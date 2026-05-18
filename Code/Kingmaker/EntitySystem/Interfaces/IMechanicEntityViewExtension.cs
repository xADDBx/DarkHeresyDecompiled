using JetBrains.Annotations;
using Kingmaker.View.Mechanics;

namespace Kingmaker.EntitySystem.Interfaces;

public static class IMechanicEntityViewExtension
{
	[CanBeNull]
	public static MechanicEntityView AsMechanicEntityView([CanBeNull] this IMechanicEntityView view)
	{
		return view as MechanicEntityView;
	}
}
