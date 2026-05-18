using DG.Tweening;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class LevelUpButtonView : MonoBehaviour
{
	[SerializeField]
	private bool m_HasAppearAnimation;

	[SerializeField]
	[ShowIf("m_HasAppearAnimation")]
	private RectMask2D m_FrameMask;

	[SerializeField]
	[ShowIf("m_HasAppearAnimation")]
	private RectMask2D m_ArrowMask;

	private Sequence m_AppearAnimation;

	private void Awake()
	{
		if (!m_HasAppearAnimation)
		{
			if (m_FrameMask != null)
			{
				m_FrameMask.gameObject.SetActive(value: false);
			}
			if (m_ArrowMask != null)
			{
				m_ArrowMask.enabled = false;
			}
		}
		else
		{
			CreateAnimation();
		}
	}

	private void OnEnable()
	{
		if (m_HasAppearAnimation && m_AppearAnimation != null)
		{
			m_AppearAnimation.Restart();
		}
	}

	private void OnDestroy()
	{
		m_AppearAnimation?.Kill();
		m_AppearAnimation = null;
	}

	private void CreateAnimation()
	{
		if (!(m_FrameMask == null) && !(m_ArrowMask == null))
		{
			float frameHeight = m_FrameMask.GetComponent<RectTransform>().rect.height;
			RectTransform arrowRect = m_ArrowMask.GetComponent<RectTransform>();
			float arrowHeight = arrowRect.rect.height;
			_ = frameHeight;
			m_FrameMask.padding = new Vector4(m_FrameMask.padding.x, 0f - frameHeight, m_FrameMask.padding.z, frameHeight);
			m_ArrowMask.padding = new Vector4(m_ArrowMask.padding.x, arrowHeight, m_ArrowMask.padding.z, 0f - arrowHeight);
			arrowRect.anchoredPosition = new Vector2(0f, -30f);
			m_AppearAnimation = DOTween.Sequence();
			m_AppearAnimation.AppendInterval(0.5f).Append(DOTween.To(() => m_FrameMask.padding.w, delegate(float x)
			{
				Vector4 padding2 = m_FrameMask.padding;
				padding2.w = x;
				padding2.y = x * -1f;
				m_FrameMask.padding = padding2;
			}, 0f - frameHeight, 0.8f)).Join(DOTween.To(() => m_ArrowMask.padding.y, delegate(float x)
			{
				Vector4 padding = m_ArrowMask.padding;
				padding.y = x;
				m_ArrowMask.padding = padding;
			}, 0f - arrowHeight, 0.8f).SetDelay(0.5f))
				.Join(arrowRect.DOAnchorPos(Vector2.zero, 0.8f))
				.SetAutoKill(autoKillOnCompletion: false)
				.OnRewind(delegate
				{
					m_FrameMask.padding = new Vector4(m_FrameMask.padding.x, 0f - frameHeight, m_FrameMask.padding.z, frameHeight);
					m_ArrowMask.padding = new Vector4(m_ArrowMask.padding.x, arrowHeight, m_ArrowMask.padding.z, 0f - arrowHeight);
					arrowRect.anchoredPosition = new Vector2(0f, -30f);
				})
				.SetUpdate(UpdateType.Normal, isIndependentUpdate: true)
				.Pause();
		}
	}
}
