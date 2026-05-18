using Kingmaker.Blueprints.Root;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM;

public class CustomUIVideoPlayerVM : ViewModel
{
	public VideoClip Video;

	public Sprite PreviewArt;

	public string SoundStart;

	public string SoundStop;

	private readonly ReactiveProperty<bool> m_HasVideo = new ReactiveProperty<bool>();

	private readonly ReactiveCommand<Unit> m_ChangeVideo = new ReactiveCommand<Unit>();

	private readonly ReactiveCommand<Unit> m_ResetVideoCommand = new ReactiveCommand<Unit>();

	public ReadOnlyReactiveProperty<bool> HasVideo => m_HasVideo;

	public Observable<Unit> ChangeVideo => m_ChangeVideo;

	public Observable<Unit> ResetVideoCommand => m_ResetVideoCommand;

	public void SetVideo(VideoClip videoClip, Sprite previewArt, string soundStart, string soundStop)
	{
		Video = videoClip;
		PreviewArt = ((previewArt != null) ? previewArt : UIConfig.Instance.KeyArt);
		SoundStart = soundStart;
		SoundStop = soundStop;
		m_HasVideo.Value = Video != null;
		m_ChangeVideo.Execute(Unit.Default);
	}

	public void ResetVideo()
	{
		m_ResetVideoCommand.Execute(Unit.Default);
	}
}
