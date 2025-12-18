using Kingmaker.UI.Common.Animations;

namespace Kingmaker.UI.Transitions;

public class UIAnimatorHideTransition : UIAnimatorTransition
{
	public UIAnimatorHideTransition(IUIAnimator animator)
		: base(animator)
	{
	}

	protected override void OnRun()
	{
		Animator.DisappearAnimation(Complete);
	}
}
