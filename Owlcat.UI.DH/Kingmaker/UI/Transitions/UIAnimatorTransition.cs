using System;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;

namespace Kingmaker.UI.Transitions;

public abstract class UIAnimatorTransition : Transition
{
	private bool m_Completed;

	private Action m_CompletedCallback;

	protected readonly IUIAnimator Animator;

	public override bool Completed => m_Completed;

	protected UIAnimatorTransition(IUIAnimator animator)
	{
		Animator = animator;
	}

	protected abstract void OnRun();

	public Transition Run(Action onCompleted = null)
	{
		m_CompletedCallback = onCompleted;
		m_Completed = false;
		OnRun();
		return this;
	}

	public override void Complete()
	{
		m_Completed = true;
		m_CompletedCallback?.Invoke();
		m_CompletedCallback = null;
	}
}
