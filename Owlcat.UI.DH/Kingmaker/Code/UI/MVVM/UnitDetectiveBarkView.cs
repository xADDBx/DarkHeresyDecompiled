using Kingmaker.Code.UI.MVVM.View;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class UnitDetectiveBarkView : BarkBlockView<UnitBarkPartVM>
{
	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.IsBarkActive.Subscribe(delegate(bool value)
		{
			FadeAnimator.PlayAnimation(value);
		}).AddTo(this);
	}
}
