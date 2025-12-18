using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyTutorialBlockView : View<NetLobbyTutorialBlockVM>
{
	[SerializeField]
	private Image m_BlockImage;

	[SerializeField]
	private TextMeshProUGUI m_BlockDescription;

	[SerializeField]
	private FadeAnimator m_BlockFadeAnimator;

	[SerializeField]
	private RectTransform m_RightArrowImage;

	protected override void OnBind()
	{
		m_BlockFadeAnimator.Initialize();
		m_BlockImage.sprite = base.ViewModel.BlockSprite;
		m_BlockDescription.text = base.ViewModel.BlockDescription;
		if (m_BlockFadeAnimator.CanvasGroup != null)
		{
			m_BlockFadeAnimator.CanvasGroup.alpha = 0f;
		}
	}

	public void ShowAnimation(bool withAnimation = false)
	{
		if (!withAnimation && m_BlockFadeAnimator.CanvasGroup != null)
		{
			m_BlockFadeAnimator.CanvasGroup.alpha = 1f;
		}
		else
		{
			m_BlockFadeAnimator.AppearAnimation();
		}
	}

	public void SetRightArrowVisible(bool state)
	{
		m_RightArrowImage.gameObject.SetActive(state);
	}
}
