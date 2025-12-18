using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CustomUIVideoPlayerConsoleView : CustomUIVideoPlayerBaseView
{
	public void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, ConsoleHint playPauseVideoHint, ReadOnlyReactiveProperty<bool> isEnabled)
	{
		playPauseVideoHint.Bind(inputLayer.AddButton(delegate
		{
			if (!VideoIsStarted)
			{
				StartVideo();
				ShowHideInterface(state: false);
			}
			else
			{
				PlayPauseVideo();
				ShowHideInterface(!VideoIsPlaying.Value);
			}
		}, 17, isEnabled.And(base.ViewModel.HasVideo).ToReadOnlyReactiveProperty(initialValue: false))).AddTo(this);
		playPauseVideoHint.SetLabel(UIStrings.Instance.DlcManager.PlayVideo);
		VideoIsPlaying.Subscribe(delegate(bool value)
		{
			playPauseVideoHint.SetLabel(value ? UIStrings.Instance.DlcManager.PauseVideo : UIStrings.Instance.DlcManager.PlayVideo);
		}).AddTo(this);
	}
}
