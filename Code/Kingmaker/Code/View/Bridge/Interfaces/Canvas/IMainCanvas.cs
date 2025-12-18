using UnityEngine;

namespace Kingmaker.Code.View.Bridge.Interfaces.Canvas;

public interface IMainCanvas
{
	RectTransform RectTransform { get; }

	CanvasGroup GetCanvasGroup();
}
