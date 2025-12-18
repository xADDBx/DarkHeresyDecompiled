using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenPortraitSelectorItemConsoleView : CharGenPortraitSelectorItemView, ILongConfirmClickHandler, IConsoleEntity
{
	public bool CanLongConfirmClick()
	{
		return m_Button.IsValid();
	}

	public void OnLongConfirmClick()
	{
	}

	public string GetLongConfirmClickHint()
	{
		return UIStrings.Instance.CharGen.ChangePortrait;
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		if (value)
		{
			EventBus.RaiseEvent(delegate(ICharGenPortraitSelectorHoverHandler h)
			{
				h.HandleHoverStart(base.ViewModel.PortraitData);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(ICharGenPortraitSelectorHoverHandler h)
			{
				h.HandleHoverStop();
			});
		}
	}
}
