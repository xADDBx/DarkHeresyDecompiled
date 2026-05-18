using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Events;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class LocalTransitionMapEntityView : TransitionMapEntityBaseView
{
	protected override void OnBind()
	{
		base.OnBind();
		base.LocationButton.OnHoverAsObservable().Subscribe(delegate(bool value)
		{
			EventBus.RaiseEvent(delegate(ITransitionMapHighlight h)
			{
				h.HandleHighlightEntry(base.ViewModel.Name, value);
			});
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.LocationButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnClick();
		}).AddTo(this);
	}
}
