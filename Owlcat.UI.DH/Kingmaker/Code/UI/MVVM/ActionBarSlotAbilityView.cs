namespace Kingmaker.Code.UI.MVVM;

public abstract class ActionBarSlotAbilityView : ActionBarBaseSlotView
{
	public int Index => base.ViewModel?.Index ?? (-1);
}
