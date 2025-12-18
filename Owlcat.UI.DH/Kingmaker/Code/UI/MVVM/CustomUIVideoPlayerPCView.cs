using Owlcat.UI;
using R3;
using R3.Triggers;

namespace Kingmaker.Code.UI.MVVM;

public class CustomUIVideoPlayerPCView : CustomUIVideoPlayerBaseView
{
	protected override void OnBind()
	{
		base.OnBind();
		ObservableSubscribeExtensions.Subscribe(m_PlayPauseBigButton.OnLeftClickAsObservable(), delegate
		{
			if (!VideoIsStarted)
			{
				StartVideo();
			}
			else
			{
				PlayPauseVideo();
			}
		}).AddTo(this);
		this.OnPointerEnterAsObservable().Subscribe(delegate
		{
			ShowHideInterface(state: true);
		}).AddTo(this);
		this.OnPointerExitAsObservable().Subscribe(delegate
		{
			ShowHideInterface(state: false);
		}).AddTo(this);
	}
}
