using System;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickTimerVM : TooltipBaseBrickVM
{
	private readonly ReactiveProperty<string> m_Text = new ReactiveProperty<string>();

	public readonly bool ShowTimeIcon;

	public ReadOnlyReactiveProperty<string> Text => m_Text;

	public TooltipBrickTimerVM(Func<string> fs, bool showIcon)
	{
		TooltipBrickTimerVM tooltipBrickTimerVM = this;
		AddDisposable(ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			tooltipBrickTimerVM.m_Text.Value = fs?.Invoke();
		}));
		ShowTimeIcon = showIcon;
	}
}
