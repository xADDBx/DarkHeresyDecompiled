using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.Code.View.UI.Components.Camera;
using Kingmaker.Code.View.UI.MVVM.DetectiveJournal.MainPage;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class DetectiveJournalBaseView : View<DetectiveJournalVM>, IInitializable
{
	[Header("Views")]
	[SerializeField]
	private DetectiveMainPageView m_MainPageView;

	[FormerlySerializedAs("OpenedCaseBaseView")]
	[SerializeField]
	private DetectiveOpenedCaseBaseView DetectiveOpenedCaseBaseView;

	[Header("Screen")]
	[SerializeField]
	private UIPostProcessMember m_UIPostProcessMember;

	[SerializeField]
	private FadeAnimator m_ScreenContentFadeAnimator;

	[SerializeField]
	private GameObject m_ShatteredGlass;

	[Header("Debug")]
	[SerializeField]
	private bool m_UsePostprocess;

	public void Initialize()
	{
		DetectiveOpenedCaseBaseView.Initialize();
	}

	protected override void OnBind()
	{
		if (m_UsePostprocess)
		{
			m_UIPostProcessMember.Bind();
		}
		base.ViewModel.MainPageVM.Subscribe(m_MainPageView.Bind).AddTo(this);
		base.ViewModel.OpenedCaseVM.Subscribe(SetOpenedCase).AddTo(this);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.DetectiveJournal);
		});
		ShowWindow();
	}

	protected override void OnUnbind()
	{
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.DetectiveJournal);
		});
		HideWindow();
	}

	private void SetOpenedCase(DetectiveOpenedCaseVM detectiveOpenedCaseVM)
	{
		if (detectiveOpenedCaseVM != null)
		{
			DetectiveOpenedCaseBaseView.Bind(detectiveOpenedCaseVM);
		}
	}

	private void ShowWindow()
	{
		base.gameObject.SetActive(value: true);
		m_ShatteredGlass.SetActive(value: true);
		UIPostProcessingAnimator.Instance.PlayState(UIPostEffectState.Default);
		m_ScreenContentFadeAnimator.AppearAnimation();
	}

	private void HideWindow()
	{
		m_ShatteredGlass.SetActive(value: false);
		if (m_UsePostprocess)
		{
			if (base.ViewModel.SwitchedFromServiceWindow.Value)
			{
				base.gameObject.SetActive(value: false);
				m_UIPostProcessMember?.Dispose();
				return;
			}
			m_ScreenContentFadeAnimator.DisappearAnimation(delegate
			{
				base.gameObject.SetActive(value: false);
				ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
				{
					m_UIPostProcessMember?.Dispose();
				});
			});
			if ((bool)UIPostProcessingAnimator.Instance)
			{
				UIPostProcessingAnimator.Instance.PlayState(UIPostEffectState.Off);
			}
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
