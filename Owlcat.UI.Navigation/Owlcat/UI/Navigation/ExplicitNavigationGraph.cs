using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UI;

namespace Owlcat.UI.Navigation;

internal class ExplicitNavigationGraph : INavigationGraph
{
	public bool TryGet([NotNull] GameObject selected, Vector2 dir, out GameObject result)
	{
		result = null;
		if (selected.TryGetComponent<Selectable>(out var component))
		{
			UnityEngine.UI.Navigation navigation = component.navigation;
			if (navigation.mode == UnityEngine.UI.Navigation.Mode.Explicit)
			{
				Selectable selectable = null;
				if (dir == Vector2.up)
				{
					selectable = navigation.selectOnUp;
				}
				else if (dir == Vector2.right)
				{
					selectable = navigation.selectOnRight;
				}
				else if (dir == Vector2.down)
				{
					selectable = navigation.selectOnDown;
				}
				else if (dir == Vector2.left)
				{
					selectable = navigation.selectOnLeft;
				}
				result = (selectable ? selectable.gameObject : null);
			}
		}
		return result != null;
	}
}
