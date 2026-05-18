using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipAimView : View<OvertipHitChanceBlockVM>
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	protected override void OnBind()
	{
		m_CanvasGroup.alpha = 0f;
		base.ViewModel.IsVisible.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).CombineLatest(base.ViewModel.CanTarget, base.ViewModel.HitAlways, base.ViewModel.EntityUIState.IsMouseOverUnit, base.ViewModel.EntityUIState.HoverSelfTargetAbility, (bool isVisible, bool canTarget, bool hitAlways, bool hover, bool selfAbility) => (isVisible && (canTarget || hitAlways) && hover) || selfAbility).Subscribe(delegate(bool b)
		{
			m_FadeAnimator.PlayAnimation(b);
		})
			.AddTo(this);
	}
}
