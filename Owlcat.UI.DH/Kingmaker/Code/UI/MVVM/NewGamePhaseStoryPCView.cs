using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhaseStoryPCView : NewGamePhaseStoryBaseView
{
	[SerializeField]
	private NewGamePhaseStoryScenarioSelectorPCView m_StorySelectorPCView;

	[SerializeField]
	private CustomUIVideoPlayerPCView m_CustomUIVideoPlayerPCView;

	public override void Initialize()
	{
		base.Initialize();
		if (!IsInit)
		{
			m_CustomUIVideoPlayerPCView.Initialize();
			IsInit = true;
		}
	}

	protected override void OnBind()
	{
		m_CustomUIVideoPlayerPCView.Bind(base.ViewModel.CustomUIVideoPlayerVM);
		base.OnBind();
		m_StorySelectorPCView.Bind(base.ViewModel.SelectionGroup);
		ObservableSubscribeExtensions.Subscribe(m_PurchaseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ShowInStore();
		}).AddTo(this);
	}

	protected override void ShowHideVideoImpl(bool state)
	{
		base.ShowHideVideoImpl(state);
		m_CustomUIVideoPlayerPCView.gameObject.SetActive(state);
	}
}
