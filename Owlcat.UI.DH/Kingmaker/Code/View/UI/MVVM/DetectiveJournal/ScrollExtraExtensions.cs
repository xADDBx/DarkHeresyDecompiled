using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public static class ScrollExtraExtensions
{
	public static Observable<PointerEventData> OnScrollAsObservable(this Component component)
	{
		if (component == null || component.gameObject == null)
		{
			return Observable.Empty<PointerEventData>();
		}
		return GetOrAddComponent<ObservableScrollTrigger>(component.gameObject).OnScrollAsObservable();
	}

	private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
	{
		T val = gameObject.GetComponent<T>();
		if (val == null)
		{
			val = gameObject.AddComponent<T>();
		}
		return val;
	}
}
