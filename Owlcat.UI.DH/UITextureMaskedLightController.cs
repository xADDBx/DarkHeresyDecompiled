using System;
using Kingmaker.UI.Pointer;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class UITextureMaskedLightController : MonoBehaviour
{
	public enum Corner
	{
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight
	}

	[Serializable]
	private class CornerSettings
	{
		public bool overrideSize;

		public Vector2 uvOffset;

		[ShowIf("overrideSize")]
		public Vector2 uvSize;
	}

	[Header("Shared Settings")]
	[SerializeField]
	private Vector2 m_SharedUVSize = new Vector2(1f, 1f);

	[SerializeField]
	private Graphic m_TargetGraphic;

	[SerializeField]
	private RectTransform m_CursorRectTransform;

	[Header("Corner Settings")]
	[SerializeField]
	private CornerSettings m_TopLeft = new CornerSettings();

	[SerializeField]
	private CornerSettings m_TopRight = new CornerSettings();

	[SerializeField]
	private CornerSettings m_BottomLeft = new CornerSettings();

	[SerializeField]
	private CornerSettings m_BottomRight = new CornerSettings();

	[Header("Preview")]
	[SerializeField]
	private bool m_PreviewInEditor;

	[SerializeField]
	private Corner m_PreviewCorner;

	private Material m_MaterialInstance;

	private static readonly int AlphaUV_ID = Shader.PropertyToID("_AlphaUV");

	private static readonly int AlphaOffset_ID = Shader.PropertyToID("_AlphaOffset");

	private void Awake()
	{
		UpdateMaterialReference();
	}

	private void Update()
	{
		if (!(m_TargetGraphic == null) && !(m_MaterialInstance == null))
		{
			Canvas canvas = m_TargetGraphic.canvas;
			if (!(canvas == null) && RectTransformUtility.ScreenPointToLocalPointInRectangle(m_CursorRectTransform, CursorController.CursorPosition, canvas.worldCamera, out var localPoint))
			{
				Rect rect = m_CursorRectTransform.rect;
				float num = Mathf.Clamp01((localPoint.x - rect.xMin) / rect.width);
				float num2 = Mathf.Clamp01((localPoint.y - rect.yMin) / rect.height);
				new Vector2(num, num2);
				Vector2 b = Vector2.Lerp(GetSize(m_TopLeft), GetSize(m_TopRight), num);
				Vector2 size = Vector2.Lerp(Vector2.Lerp(GetSize(m_BottomLeft), GetSize(m_BottomRight), num), b, num2);
				Vector2 b2 = Vector2.Lerp(m_TopLeft.uvOffset, m_TopRight.uvOffset, num);
				Vector2 offset = Vector2.Lerp(Vector2.Lerp(m_BottomLeft.uvOffset, m_BottomRight.uvOffset, num), b2, num2);
				Apply(size, offset);
			}
		}
	}

	private void ApplyCorner(CornerSettings settings)
	{
		Vector2 size = GetSize(settings);
		Apply(size, settings.uvOffset);
	}

	private void Apply(Vector2 size, Vector2 offset)
	{
		if (m_MaterialInstance != null)
		{
			m_MaterialInstance.SetVector(AlphaUV_ID, size);
			m_MaterialInstance.SetVector(AlphaOffset_ID, offset);
		}
	}

	private Vector2 GetSize(CornerSettings corner)
	{
		if (!corner.overrideSize)
		{
			return m_SharedUVSize;
		}
		return corner.uvSize;
	}

	private CornerSettings GetCornerSettings(Corner corner)
	{
		return corner switch
		{
			Corner.TopLeft => m_TopLeft, 
			Corner.TopRight => m_TopRight, 
			Corner.BottomLeft => m_BottomLeft, 
			Corner.BottomRight => m_BottomRight, 
			_ => m_TopLeft, 
		};
	}

	private void UpdateMaterialReference()
	{
		if (m_TargetGraphic == null)
		{
			m_MaterialInstance = null;
		}
		else
		{
			m_MaterialInstance = (Application.isPlaying ? m_TargetGraphic.material : m_TargetGraphic.materialForRendering);
		}
	}
}
