using DG.Tweening;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoBookmarkButton : MonoBehaviour
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private RectTransform m_Transform;

	[SerializeField]
	private float m_ActiveYPos;

	[SerializeField]
	private float m_InactiveYPos;

	[SerializeField]
	private float m_TweenDuration = 0.22f;

	[SerializeField]
	private Ease m_TweenEase = Ease.OutCubic;

	private Tween m_MoveTween;

	private bool m_IsActive;

	public OwlcatMultiButton Button => m_Button;

	public void SetActiveState(bool isActive)
	{
		m_MoveTween?.Pause();
		m_IsActive = isActive;
		m_Button.Interactable = isActive;
		string activeLayer = (isActive ? "Enabled" : "Disabled");
		m_Button.SetActiveLayer(activeLayer);
		Vector2 anchoredPosition = m_Transform.anchoredPosition;
		anchoredPosition.y = (isActive ? m_ActiveYPos : m_InactiveYPos);
		m_Transform.anchoredPosition = anchoredPosition;
	}

	private void HandleButtonHovered(bool isHovered)
	{
		if (!m_IsActive)
		{
			if (m_MoveTween == null)
			{
				m_MoveTween = m_Transform.DOAnchorPosY(m_ActiveYPos, m_TweenDuration).SetEase(m_TweenEase).SetAutoKill(autoKillOnCompletion: false)
					.SetUpdate(UpdateType.Normal, isIndependentUpdate: true);
			}
			if (isHovered)
			{
				m_MoveTween.PlayForward();
			}
			else
			{
				m_MoveTween.PlayBackwards();
			}
		}
	}

	private void Awake()
	{
		m_Button.OnHoverAsObservable().Subscribe(HandleButtonHovered).AddTo(this);
	}

	private void OnDestroy()
	{
		m_MoveTween?.Kill();
	}
}
