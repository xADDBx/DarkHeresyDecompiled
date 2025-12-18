using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoComponentWithLevelUpView<TViewModel> : CharInfoComponentView<TViewModel> where TViewModel : CharInfoComponentWithLevelUpVM
{
	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.PreviewUnit.Subscribe(delegate
		{
			RefreshView();
		}).AddTo(this);
	}
}
