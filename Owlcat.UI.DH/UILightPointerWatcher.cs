using Kingmaker.UI.Pointer;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UILightPointerWatcher : MonoBehaviour
{
	[Header("Диапазон движения курсора (в пикселях)")]
	private Vector2 screenMin = new Vector2(0f, 0f);

	private Vector2 screenMax = new Vector2(Screen.width, Screen.height);

	[Header("Диапазон движения UI-элемента")]
	public Vector2 uiMin = new Vector2(-100f, 100f);

	public Vector2 uiMax = new Vector2(100f, -100f);

	private RectTransform rectTransform;

	private void Start()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	private void Update()
	{
		Vector2 cursorPosition = CursorController.CursorPosition;
		screenMin = new Vector2(0f, 0f);
		screenMax = new Vector2(Screen.width, Screen.height);
		float t = Mathf.InverseLerp(screenMin.x, screenMax.x, cursorPosition.x);
		float t2 = Mathf.InverseLerp(screenMin.y, screenMax.y, cursorPosition.y);
		float x = Mathf.Lerp(uiMin.x, uiMax.x, t);
		float y = Mathf.Lerp(uiMin.y, uiMax.y, t2);
		rectTransform.anchoredPosition = new Vector2(x, y);
	}
}
