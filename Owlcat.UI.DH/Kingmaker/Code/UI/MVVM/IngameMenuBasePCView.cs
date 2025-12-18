using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class IngameMenuBasePCView<TViewModel> : View<TViewModel> where TViewModel : IngameMenuBaseVM
{
	[SerializeField]
	private FadeAnimator m_Animator;

	public virtual void Awake()
	{
	}

	protected override void OnBind()
	{
		base.ViewModel.ShouldShow.Subscribe(SwitchVisibility).AddTo(this);
	}

	protected override void OnUnbind()
	{
	}

	private void SwitchVisibility(bool state)
	{
		if (state)
		{
			m_Animator.AppearAnimation();
			return;
		}
		m_Animator.DisappearAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}
}
