using DG.Tweening;
using Kingmaker.AreaLogic.Cutscenes.Commands;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.Utility;
using Kingmaker.Visual.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM;

public class InterchapterBaseView : View<InterchapterVM>, IInitializable
{
	[Header("Video")]
	[SerializeField]
	private VideoPlayerHelper m_Video;

	[FormerlySerializedAs("m_VideoBackground")]
	[SerializeField]
	private CanvasGroup m_VideoGroup;

	[Header("Subtitle")]
	[SerializeField]
	private TextMeshProUGUI m_SubtitleText;

	[SerializeField]
	private CanvasGroup m_SubtitleGroup;

	private readonly ReactiveProperty<VideoState> m_State = new ReactiveProperty<VideoState>(VideoState.Inactive);

	private float m_InterruptTimer;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_State.Value = VideoState.Inactive;
		m_Video.Initialize();
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate), delegate
		{
			InternalUpdate();
		}).AddTo(this);
		m_State.Subscribe(delegate(VideoState value)
		{
			base.ViewModel.Data.SetState(value);
		}).AddTo(this);
		m_State.Value = VideoState.Preparing;
		m_VideoGroup.alpha = 0f;
		m_VideoGroup.DOFade(1f, 0.2f).SetUpdate(isIndependentUpdate: true);
		m_Video.Stop();
		m_Video.SetClip(base.ViewModel.VideoClip, SoundStateType.Video, prepareVideo: true, base.ViewModel.SoundStartEvent, base.ViewModel.SoundStopEvent);
		base.ViewModel.Subtitle.Subscribe(delegate(string value)
		{
			m_SubtitleGroup.DOFade((value != string.Empty) ? 1 : 0, 0.2f).SetUpdate(isIndependentUpdate: true);
			m_SubtitleText.text = value;
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_Video.Stop();
		m_State.Value = VideoState.Inactive;
		base.gameObject.SetActive(value: false);
	}

	private void InternalUpdate()
	{
		switch (m_State.Value)
		{
		case VideoState.Preparing:
			if (m_Video.IsPlaying)
			{
				m_State.Value = VideoState.Playing;
			}
			break;
		case VideoState.Playing:
			if (!m_Video.IsPlaying || m_Video.IsOvertime)
			{
				m_State.Value = VideoState.Finishing;
			}
			break;
		case VideoState.PlayingPressAnyKey:
			m_InterruptTimer -= Time.deltaTime;
			if (m_InterruptTimer <= 0f)
			{
				m_State.Value = VideoState.Playing;
			}
			if (!m_Video.IsPlaying)
			{
				m_State.Value = VideoState.Finishing;
			}
			break;
		case VideoState.Finishing:
			break;
		}
	}
}
