using JetBrains.Annotations;
using Kingmaker.Code.View.Bridge.Interfaces.Canvas;
using UnityEngine;

namespace Kingmaker.UI.Canvases;

public class MainCanvas : MonoBehaviour, IMainCanvas
{
	private RectTransform m_RectTransform;

	private CanvasGroup m_CanvasGroup;

	public static MainCanvas Instance { get; private set; }

	public RectTransform RectTransform => m_RectTransform ?? (m_RectTransform = GetComponent<RectTransform>());

	public CanvasGroup GetCanvasGroup()
	{
		return m_CanvasGroup ?? (m_CanvasGroup = GetComponent<CanvasGroup>());
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		Instance = this;
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		Instance = null;
	}
}
