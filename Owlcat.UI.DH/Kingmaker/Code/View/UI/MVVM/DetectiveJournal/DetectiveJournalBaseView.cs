using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.Code.View.UI.MVVM.DetectiveJournal.MainPage;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
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
	private UIServiceWindowPostProcessView m_PostProcessView;

	public void Initialize()
	{
		DetectiveOpenedCaseBaseView.Initialize();
		m_PostProcessView.Initialize();
	}

	protected override void OnBind()
	{
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
		m_PostProcessView.ShowFrom(RootVM.Instance.ServiceWindowsContext.HasPrevWindow ? UIPostEffectState.Default : UIPostEffectState.Off);
	}

	private void HideWindow()
	{
		m_PostProcessView.Hide(base.ViewModel.SwitchedFromServiceWindow.Value, delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}
}
