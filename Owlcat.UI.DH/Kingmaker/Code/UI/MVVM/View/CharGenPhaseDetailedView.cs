using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public abstract class CharGenPhaseDetailedView<TViewModel> : View<TViewModel>, ICharGenPhaseDetailedView, IInitializable where TViewModel : CharGenPhaseBaseVM
{
	[SerializeField]
	private FadeAnimator m_PageAnimator;

	protected readonly ReactiveProperty<bool> CanGoBackOnDecline = new ReactiveProperty<bool>(value: true);

	protected readonly ReactiveProperty<bool> CanGoNextInMenu = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<bool> CanGoNextOnConfirm = new ReactiveProperty<bool>();

	protected PaperHints PaperHints;

	protected virtual bool HasYScrollBindInternal => true;

	public bool HasYScrollBind => HasYScrollBindInternal;

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	public abstract void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, ReadOnlyReactiveProperty<bool> isMainCharacter);

	public ReadOnlyReactiveProperty<bool> GetCanGoNextOnConfirmProperty()
	{
		return CanGoNextOnConfirm;
	}

	public virtual ReadOnlyReactiveProperty<bool> CanGoNextInMenuProperty()
	{
		return CanGoNextOnConfirm;
	}

	public ReadOnlyReactiveProperty<bool> GetCanGoBackOnDeclineProperty()
	{
		return CanGoBackOnDecline;
	}

	public virtual bool PressConfirmOnPhase()
	{
		return base.ViewModel.IsCompletedAndAvailable.CurrentValue;
	}

	public virtual bool PressDeclineOnPhase()
	{
		return true;
	}

	public void SetPaperHints(PaperHints paperHints)
	{
		PaperHints = paperHints;
	}

	protected override void OnBind()
	{
		if (base.ViewModel == null)
		{
			OnUnbind();
		}
		else
		{
			Show();
		}
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	private void Show()
	{
		if ((bool)m_PageAnimator)
		{
			m_PageAnimator.AppearAnimation();
		}
	}

	private void Hide()
	{
		if ((bool)m_PageAnimator)
		{
			m_PageAnimator.DisappearAnimation();
		}
	}
}
