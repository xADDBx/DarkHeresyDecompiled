using System;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class TransitionLegendButtonVM : ViewModel
{
	public bool IsVisible;

	public string Name;

	private readonly ReactiveProperty<bool> m_Attention = new ReactiveProperty<bool>();

	public Action CloseAction;

	public Action ClickAction;

	public Action HoverAction;

	public Action UnHoverAction;

	public readonly TransitionEntryVM TransitionEntryVM;

	private readonly ReactiveProperty<bool> m_IsHover = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> Attention => m_Attention;

	public ReadOnlyReactiveProperty<bool> IsHover => m_IsHover;

	public TransitionLegendButtonVM(TransitionEntryVM transitionEntryVM, Action hoverAction, Action unHoverAction)
	{
		m_Attention.Value = transitionEntryVM.Attention.CurrentValue;
		IsVisible = transitionEntryVM.IsVisible.CurrentValue && transitionEntryVM.IsInteractable.CurrentValue;
		Name = transitionEntryVM.Name.CurrentValue;
		TransitionEntryVM = transitionEntryVM;
		CloseAction = transitionEntryVM.CloseAction;
		ClickAction = transitionEntryVM.ClickAction;
		HoverAction = hoverAction;
		UnHoverAction = unHoverAction;
	}

	public void OnClick()
	{
		if (UtilityNet.IsControlMainCharacterWithWarning())
		{
			ClickAction?.Invoke();
		}
	}

	public void SetHover(bool isHover)
	{
		m_IsHover.Value = isHover;
	}
}
