using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(UnifiedCursor))]
public class CursorView : View<CursorVM>
{
	[Header("Elements")]
	[SerializeField]
	private RectTransform m_SoftRect;

	[SerializeField]
	private RawImage m_SoftImage;

	[SerializeField]
	private Image m_IconImage;

	[Header("Text")]
	[SerializeField]
	private CursorTextWidget m_CursorText;

	[Header("Settings")]
	[SerializeField]
	private CursorRoot m_Textures;

	[SerializeReference]
	[MaybeNull]
	[UsedImplicitly]
	private CursorVM m_ViewModel;

	private RectTransform m_RectTransform;

	private Canvas m_RootCanvas;

	private UnifiedCursor m_Cursor;

	private readonly Vector3[] m_CanvasCornersScratch = new Vector3[4];

	private float m_CachedCanvasPixelsPerUnit;

	private bool m_CanvasPixelsPerUnitDirty = true;

	private void Awake()
	{
		RectTransform component = GetComponent<RectTransform>();
		component.anchorMin = Vector2.zero;
		component.anchorMax = Vector2.one;
		component.anchoredPosition = Vector2.zero;
		component.sizeDelta = Vector2.zero;
	}

	protected override void OnBind()
	{
		m_RectTransform = GetComponent<RectTransform>();
		m_RootCanvas = GetComponentInParent<Canvas>().rootCanvas;
		m_Cursor = GetComponent<UnifiedCursor>();
		base.ViewModel.TextUpper.CombineLatest(base.ViewModel.TextLower, (string upper, string lower) => new { upper, lower }).Subscribe(data =>
		{
			m_CursorText.SetText(data.upper, data.lower);
		}).AddTo(this);
		m_ViewModel = base.ViewModel;
	}

	private bool NeedSoftwareCursor()
	{
		if (m_Cursor.HasNativeMouse && !base.ViewModel.Software.CurrentValue && !base.ViewModel.Icon.CurrentValue && string.IsNullOrEmpty(base.ViewModel.TextUpper.CurrentValue) && string.IsNullOrEmpty(base.ViewModel.TextLower.CurrentValue))
		{
			return !Mathf.Approximately(base.ViewModel.Scale.CurrentValue, 1f);
		}
		return true;
	}

	private void LateUpdate()
	{
		m_Cursor.enabled = base.ViewModel.Enabled.CurrentValue;
		if (HasCursorInScreenSpace())
		{
			bool flag = !NeedSoftwareCursor();
			m_SoftRect.gameObject.SetActive(!flag);
			if (flag && m_Textures.TryGetTexture(base.ViewModel.Type.CurrentValue, out var texture, out var pivot))
			{
				Cursor.SetCursor(texture, new Vector2((float)texture.width * pivot.x, (float)texture.height * (1f - pivot.y)), CursorMode.Auto);
				return;
			}
			m_Textures.TryGetTexture(CursorType.Invisible, out var texture2, out var _);
			Cursor.SetCursor(texture2, Vector2.zero, CursorMode.ForceSoftware);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RectTransform, m_Cursor.Position, m_RootCanvas.worldCamera, out var localPoint);
			m_SoftRect.localPosition = localPoint;
			m_SoftRect.localScale = GetScale() * Vector3.one;
			if (m_Textures.TryGetTexture(base.ViewModel.Type.CurrentValue, out var texture3, out pivot))
			{
				m_SoftImage.texture = texture3;
				m_SoftRect.pivot = pivot;
			}
			if (m_IconImage != null)
			{
				m_IconImage.sprite = base.ViewModel.Icon.CurrentValue;
				m_IconImage.gameObject.SetActive(base.ViewModel.Icon.CurrentValue != null);
			}
		}
		else
		{
			m_SoftRect.gameObject.SetActive(value: false);
			Cursor.visible = true;
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		}
	}

	private bool HasCursorInScreenSpace()
	{
		if (m_Cursor.enabled && m_Cursor.Position.x >= 0f && m_Cursor.Position.x <= (float)Screen.width && m_Cursor.Position.y >= 0f)
		{
			return m_Cursor.Position.y <= (float)Screen.height;
		}
		return false;
	}

	private float GetScale()
	{
		float num = base.ViewModel.Scale.CurrentValue / GetCanvasPixelsPerUnit();
		RuntimePlatform platform = Application.platform;
		if ((uint)platform <= 1u)
		{
			num *= 2f;
		}
		return num;
	}

	private float GetCanvasPixelsPerUnit()
	{
		if (!m_CanvasPixelsPerUnitDirty)
		{
			return m_CachedCanvasPixelsPerUnit;
		}
		m_CachedCanvasPixelsPerUnit = CalculateCanvasPixelsPerUnit();
		m_CanvasPixelsPerUnitDirty = false;
		return m_CachedCanvasPixelsPerUnit;
	}

	private float CalculateCanvasPixelsPerUnit()
	{
		float scaleFactor = m_RootCanvas.scaleFactor;
		if (m_RootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
		{
			return scaleFactor;
		}
		RectTransform rectTransform = m_RootCanvas.transform as RectTransform;
		Camera worldCamera = m_RootCanvas.worldCamera;
		if (rectTransform == null || worldCamera == null)
		{
			return scaleFactor;
		}
		rectTransform.GetWorldCorners(m_CanvasCornersScratch);
		Vector3 vector = worldCamera.WorldToScreenPoint(m_CanvasCornersScratch[0]);
		Vector3 vector2 = worldCamera.WorldToScreenPoint(m_CanvasCornersScratch[3]);
		float width = rectTransform.rect.width;
		if (width <= Mathf.Epsilon)
		{
			return scaleFactor;
		}
		return Mathf.Abs(vector2.x - vector.x) / width;
	}

	private void OnRectTransformDimensionsChange()
	{
		m_CanvasPixelsPerUnitDirty = true;
	}
}
