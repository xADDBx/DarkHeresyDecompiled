using System;

namespace Owlcat.UI.Navigation;

public static class FocusLayerNavigationExtensions
{
	public static IDisposable AddNavigation<T>(this View<T> view, NavigationSettings settings = default(NavigationSettings))
	{
		if (view.TryGetComponent<FocusLayer>(out var component))
		{
			return new FocusLayerNavigation(component, settings);
		}
		throw new Exception(string.Format("{0} is missing on {1}", "FocusLayer", view));
	}
}
