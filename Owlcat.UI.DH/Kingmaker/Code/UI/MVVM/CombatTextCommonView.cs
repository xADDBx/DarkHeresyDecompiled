using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CombatTextCommonView : CombatTextEntityBaseView<CombatMessageBase>
{
	[Serializable]
	public class CombatTextBigIconAnimationParams
	{
		public float RotationAmount = 30f;

		public float ScaleAmount = 1.2f;

		public float AnimationDuration = 0.5f;
	}

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Image m_IconContainer;

	[SerializeField]
	private CanvasGroup m_IconCanvasGroup;

	[SerializeField]
	private Image m_BigIcon;

	[SerializeField]
	private RectTransform m_BigIconContainer;

	[SerializeField]
	private CanvasGroup m_BigIconCanvasGroup;

	[SerializeField]
	private CombatTextBigIconAnimationParams m_BigIconAnimParams;

	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private Color m_DefaultColor;

	[SerializeField]
	private float m_Spacing = 6f;

	[SerializeField]
	private Image m_Strikethrough;

	[SerializeField]
	private float m_StrikethroughHeight = 4f;

	[SerializeField]
	private float m_StrikethroughMarginLeft = 4f;

	[SerializeField]
	private float m_StrikethroughMarginRight = 4f;

	[SerializeField]
	private float m_MaxWidth = 370f;

	private bool m_IsStrikethrough;

	private bool m_HasBigIcon;

	private Sequence m_AnimationSequence;

	private Vector3 m_BigIconOriginalScale;

	public override void Dispose()
	{
		base.Dispose();
		if (m_AnimationSequence != null)
		{
			m_AnimationSequence.Kill();
			m_AnimationSequence = null;
		}
	}

	protected override float GetXPos()
	{
		if (!(m_Icon.sprite != null))
		{
			return 0f;
		}
		return ((RectTransform)m_IconContainer.transform).rect.width / 2f + m_Spacing;
	}

	protected override void DoData(CombatMessageBase combatMessage)
	{
		m_Text.text = combatMessage.GetText();
		m_Text.color = combatMessage.GetColor() ?? m_DefaultColor;
		m_Text.textWrappingMode = ((m_Text.preferredWidth >= m_MaxWidth) ? TextWrappingModes.Normal : TextWrappingModes.NoWrap);
		float num = Mathf.Min(m_Text.preferredWidth, m_MaxWidth);
		m_Text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num);
		base.Rect.sizeDelta = new Vector2(num + m_IconContainer.rectTransform.rect.width + m_Spacing * 2f, m_Text.preferredHeight);
		Vector3 localPosition = base.Rect.localPosition;
		localPosition.x = 0f;
		base.Rect.localPosition = localPosition;
		m_Icon.sprite = combatMessage.GetSprite();
		m_Icon.color = ((m_Icon.sprite != null) ? Color.white : Color.clear);
		m_IconCanvasGroup.alpha = ((m_Icon.sprite != null) ? 1f : 0f);
		m_IsStrikethrough = combatMessage.GetStrikethrough();
		Sprite bigSprite = combatMessage.GetBigSprite();
		m_HasBigIcon = bigSprite != null;
		m_BigIcon.sprite = bigSprite;
		m_BigIconCanvasGroup.alpha = ((bigSprite != null) ? 1f : 0f);
	}

	protected override void DoShow()
	{
		base.CanvasGroup.alpha = 0f;
		base.CanvasGroup.DOFade(1f, ShowFadeTime).SetUpdate(isIndependentUpdate: true);
		ShowStrikethrough();
		ShowBigIcon();
	}

	private void ShowStrikethrough()
	{
		RectTransform rectTransform = m_Strikethrough.rectTransform;
		rectTransform.sizeDelta = new Vector2(0f, m_StrikethroughHeight);
		if (m_IsStrikethrough)
		{
			float x = m_Text.rectTransform.sizeDelta.x - m_StrikethroughMarginLeft - m_StrikethroughMarginRight;
			rectTransform.DOSizeDelta(new Vector2(x, m_StrikethroughHeight), ShowFadeTime).SetDelay(ShowFadeTime).SetEase(Ease.InOutQuad)
				.SetUpdate(isIndependentUpdate: true);
		}
	}

	private void ShowBigIcon()
	{
		if (m_HasBigIcon)
		{
			Vector3 originalScale = m_BigIconContainer.localScale;
			m_AnimationSequence = DOTween.Sequence();
			m_AnimationSequence.Append(m_BigIconContainer.DORotate(new Vector3(0f, 0f, m_BigIconAnimParams.RotationAmount), m_BigIconAnimParams.AnimationDuration / 4f).SetEase(Ease.InOutSine)).Append(m_BigIconContainer.DORotate(new Vector3(0f, 0f, 0f - m_BigIconAnimParams.RotationAmount), m_BigIconAnimParams.AnimationDuration / 2f).SetEase(Ease.InOutSine)).Append(m_BigIconContainer.DORotate(Vector3.zero, m_BigIconAnimParams.AnimationDuration / 4f).SetEase(Ease.InOutSine));
			m_AnimationSequence.Join(m_BigIconContainer.DOScale(originalScale * m_BigIconAnimParams.ScaleAmount, m_BigIconAnimParams.AnimationDuration / 2f).SetEase(Ease.InOutQuad)).Append(m_BigIconContainer.DOScale(originalScale, m_BigIconAnimParams.AnimationDuration / 2f).SetEase(Ease.InOutQuad));
			m_AnimationSequence.OnKill(delegate
			{
				m_BigIconContainer.localScale = originalScale;
			});
			m_AnimationSequence.SetLoops(-1).SetUpdate(isIndependentUpdate: true);
		}
	}
}
