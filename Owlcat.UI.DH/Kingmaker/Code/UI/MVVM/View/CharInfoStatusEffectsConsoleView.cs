using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharInfoStatusEffectsConsoleView : CharInfoStatusEffectsView
{
	private void OnFocusChanged(IConsoleEntity focus)
	{
		if (focus != null)
		{
			RectTransform rectTransform = ((focus as MonoBehaviour) ?? (focus as IMonoBehaviour)?.MonoBehaviour)?.transform as RectTransform;
			EnsureScrollRect(rectTransform);
		}
	}
}
