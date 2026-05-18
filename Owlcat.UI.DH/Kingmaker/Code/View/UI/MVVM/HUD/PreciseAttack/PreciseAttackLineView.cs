using System;
using DG.Tweening;
using Kingmaker.Code.View.UI.UIUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.HUD.PreciseAttack;

public class PreciseAttackLineView : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_Line;

	[SerializeField]
	private Image m_LineImage;

	[SerializeField]
	private bool m_FreezePosition;

	private RectTransform m_RectTransform;

	private Rect m_ParentRect;

	private int m_LastScreenWidth = -1;

	private int m_LastScreenHeight = -1;

	private static Rect s_CanvasRect = new Rect(-0.2f, -0.2f, 1.2f, 1.2f);

	private Transform m_Bone;

	private Vector3 m_BoneWorldPositionCached;

	private RectTransform m_Start;

	private Color m_LineColor;

	private bool m_IsActive;

	private Vector2 m_From;

	private Vector2 m_To;

	public void SetLine(Transform locatorTransform, RectTransform start)
	{
		if (locatorTransform == null)
		{
			HideLine();
			return;
		}
		m_IsActive = true;
		m_Bone = locatorTransform;
		m_BoneWorldPositionCached = locatorTransform.position;
		m_Start = start;
		m_LineImage.color = m_LineColor;
		m_LineImage.DOKill();
		m_LineImage.fillAmount = 0f;
		m_LineImage.DOFillAmount(1f, 0.3f).SetDelay(0.1f).SetEase(Ease.OutQuad)
			.SetUpdate(isIndependentUpdate: true)
			.SetAutoKill(autoKillOnCompletion: true);
		UpdatePosition();
	}

	public void UpdatePosition()
	{
		if (m_IsActive)
		{
			Vector3? currentCanvasPosition = GetCurrentCanvasPosition();
			if (currentCanvasPosition.HasValue && m_IsActive)
			{
				UpdatePositionInternal(currentCanvasPosition.Value);
				DrawInternal();
			}
		}
	}

	public void SetColor(Color color)
	{
		m_LineColor = color;
		if (m_IsActive)
		{
			m_LineImage.color = color;
		}
	}

	private void HideLine()
	{
		m_IsActive = false;
		m_LineImage.color = Color.clear;
	}

	private void UpdateParentRect()
	{
		m_ParentRect = ((RectTransform)base.transform.parent).rect;
		m_LastScreenWidth = Screen.width;
		m_LastScreenHeight = Screen.height;
	}

	private void UpdatePositionInternal(Vector3 canvasPosition)
	{
		if (Screen.width != m_LastScreenWidth || Screen.height != m_LastScreenHeight)
		{
			UpdateParentRect();
		}
		m_To = UIUtilityRect.NormalizedToPixelPosition(m_ParentRect, canvasPosition);
		m_To -= new Vector2(m_ParentRect.width / 2f, m_ParentRect.height / 2f);
		m_From = m_Start.position;
	}

	private Vector3? GetCurrentCanvasPosition()
	{
		Vector3 normalizedPositionInCamera = UIUtilityRect.GetNormalizedPositionInCamera(m_FreezePosition ? m_BoneWorldPositionCached : m_Bone.position);
		if (s_CanvasRect.Contains(normalizedPositionInCamera))
		{
			return normalizedPositionInCamera;
		}
		return null;
	}

	private void DrawInternal()
	{
		float x = Vector3.Distance((Vector3)m_From, (Vector3)m_To);
		Vector2 vector = m_To - m_From;
		float z = (MathF.PI + Mathf.Atan2(vector.y, vector.x)) * 57.29578f;
		m_Line.transform.position = new Vector3(m_From.x, m_From.y, 2500f);
		m_Line.eulerAngles = new Vector3(0f, 0f, z);
		m_Line.sizeDelta = new Vector2(x, m_Line.sizeDelta.y);
	}

	private void Awake()
	{
		if ((object)m_RectTransform == null)
		{
			m_RectTransform = GetComponent<RectTransform>();
		}
		m_LineColor = m_LineImage.color;
	}
}
