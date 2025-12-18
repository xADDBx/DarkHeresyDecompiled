using Kingmaker.Code.UI.MVVM.View;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipBarkBlockView : BarkBlockView<OvertipBarkBlockVM>
{
	[Header("Elements")]
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.IsBarkActive.Subscribe(delegate(bool value)
		{
			FadeAnimator.PlayAnimation(value);
		}).AddTo(this);
		SetCanvasGroupIgnoreState(state: true);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		SetCanvasGroupIgnoreState(state: false);
	}

	private void SetCanvasGroupIgnoreState(bool state)
	{
		if (m_CanvasGroup != null)
		{
			m_CanvasGroup.ignoreParentGroups = state;
		}
	}
}
