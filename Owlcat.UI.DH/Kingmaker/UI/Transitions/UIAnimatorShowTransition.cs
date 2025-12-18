using Kingmaker.UI.Common.Animations;

namespace Kingmaker.UI.Transitions;

public class UIAnimatorShowTransition : UIAnimatorTransition
{
	public UIAnimatorShowTransition(IUIAnimator animator)
		: base(animator)
	{
	}

	protected override void OnRun()
	{
		Animator.AppearAnimation(Complete);
	}
}
