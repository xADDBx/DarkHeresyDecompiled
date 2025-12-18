using Kingmaker.UI.InputSystems;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class VariativeInteractionPCView : VariativeInteractionView
{
	protected override void OnBind()
	{
		base.OnBind();
		EscHotkeyManager.Instance.Subscribe(delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
	}
}
