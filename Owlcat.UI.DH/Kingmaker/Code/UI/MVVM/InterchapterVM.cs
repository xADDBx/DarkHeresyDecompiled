using Kingmaker.AreaLogic.Cutscenes.Commands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM;

public class InterchapterVM : ViewModel, ISubtitleBarkHandler, ISubscriber
{
	private readonly ReactiveProperty<string> m_Subtitle = new ReactiveProperty<string>(string.Empty);

	public InterchapterData Data { get; }

	public ReadOnlyReactiveProperty<string> Subtitle => m_Subtitle;

	public VideoClip VideoClip => Data.VideoClip;

	public string SoundStartEvent => Data.SoundStartEvent;

	public string SoundStopEvent => Data.SoundStopEvent;

	public InterchapterVM(InterchapterData data)
	{
		EventBus.Subscribe(this).AddTo(this);
		Data = data;
	}

	public void Finish()
	{
		Data.Finish();
	}

	public void HandleOnShowBark(string text)
	{
		m_Subtitle.Value = text;
	}

	public void HandleOnHideBark()
	{
		m_Subtitle.Value = string.Empty;
	}
}
