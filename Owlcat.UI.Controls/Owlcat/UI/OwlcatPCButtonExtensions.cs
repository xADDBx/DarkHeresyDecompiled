using R3;

namespace Owlcat.UI;

public static class OwlcatPCButtonExtensions
{
	public static Observable<Unit> OnLeftClickAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnLeftClick.AsObservable();
	}

	public static Observable<Unit> OnRightClickAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnRightClick.AsObservable();
	}

	public static Observable<Unit> OnSingleLeftClickAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnSingleLeftClick.AsObservable();
	}

	public static Observable<Unit> OnSingleRightClickAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnSingleRightClick.AsObservable();
	}

	public static Observable<Unit> OnLeftDoubleClickAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnLeftDoubleClick.AsObservable();
	}

	public static Observable<Unit> OnRightDoubleClickAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnRightDoubleClick.AsObservable();
	}

	public static Observable<Unit> OnLeftClickAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnLeftClick.AsObservable();
	}

	public static Observable<Unit> OnRightClickAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnRightClick.AsObservable();
	}

	public static Observable<Unit> OnSingleLeftClickAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnSingleLeftClick.AsObservable();
	}

	public static Observable<Unit> OnSingleRightClickAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnSingleRightClick.AsObservable();
	}

	public static Observable<Unit> OnLeftDoubleClickAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnLeftDoubleClick.AsObservable();
	}

	public static Observable<Unit> OnRightDoubleClickAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnRightDoubleClick.AsObservable();
	}

	public static Observable<Unit> OnLeftClickNotInteractableAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnLeftClickNotInteractable.AsObservable();
	}

	public static Observable<Unit> OnRightClickNotInteractableAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnRightClickNotInteractable.AsObservable();
	}

	public static Observable<Unit> OnSingleLeftClickNotInteractableAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnSingleLeftClickNotInteractable.AsObservable();
	}

	public static Observable<Unit> OnSingleRightClickNotInteractableAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnSingleRightClickNotInteractable.AsObservable();
	}

	public static Observable<Unit> OnLeftDoubleClickNotInteractableAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnLeftDoubleClickNotInteractable.AsObservable();
	}

	public static Observable<Unit> OnRightDoubleClickNotInteractableAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnRightDoubleClickNotInteractable.AsObservable();
	}

	public static Observable<Unit> OnLeftClickNotInteractableAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnLeftClickNotInteractable.AsObservable();
	}

	public static Observable<Unit> OnRightClickNotInteractableAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnRightClickNotInteractable.AsObservable();
	}

	public static Observable<Unit> OnSingleLeftClickNotInteractableAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnSingleLeftClickNotInteractable.AsObservable();
	}

	public static Observable<Unit> OnSingleRightClickNotInteractableAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnSingleRightClickNotInteractable.AsObservable();
	}

	public static Observable<Unit> OnLeftDoubleClickNotInteractableAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnLeftDoubleClickNotInteractable.AsObservable();
	}

	public static Observable<Unit> OnRightDoubleClickNotInteractableAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnRightDoubleClickNotInteractable.AsObservable();
	}

	public static Observable<bool> OnHoverAsObservable(this OwlcatSelectable selectable)
	{
		if (selectable == null)
		{
			return Observable.Empty<bool>();
		}
		return selectable.OnHover.AsObservable();
	}
}
