using JetBrains.Annotations;
using Kingmaker.View.MapObjects;

namespace Kingmaker.EntitySystem.Interfaces;

public static class IMapObjectViewExtension
{
	[CanBeNull]
	public static MapObjectView AsMapObjectView([CanBeNull] this IMechanicEntityView view)
	{
		return view as MapObjectView;
	}
}
