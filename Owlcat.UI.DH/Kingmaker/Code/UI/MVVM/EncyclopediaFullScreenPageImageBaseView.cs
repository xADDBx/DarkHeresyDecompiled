using System;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaFullScreenPageImageBaseView : View<EncyclopediaPageImageVM>
{
	[SerializeField]
	private Image m_Image;

	[SerializeField]
	protected OwlcatButton m_CloseButton;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	private Action m_CloseAction;

	public void Initialize(Action closeAction)
	{
		m_CloseAction = closeAction;
		m_FadeAnimator.Initialize();
	}

	protected override void OnBind()
	{
		m_Image.sprite = base.ViewModel.Image;
		m_FadeAnimator.AppearAnimation();
	}

	public void Close()
	{
		m_FadeAnimator.DisappearAnimation(delegate
		{
			m_CloseAction?.Invoke();
		});
	}
}
