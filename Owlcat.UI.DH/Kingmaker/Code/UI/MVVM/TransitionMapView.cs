using Code.View.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TransitionMapView : View<TransitionMapVM>, IFullScreenUIHandler, ISubscriber
{
	[Header("Elements")]
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	[SerializeField]
	private TransformControlsWidget m_TransformControls;

	[Header("Views")]
	[SerializeField]
	private EnumToObjectSelector<BlueprintMultiEntranceMap, TransitionMapBoardBaseView> m_TransitionMapLocalViews;

	private bool m_GroupChangerOpen;

	protected override void OnBind()
	{
		m_TransformControls.Bind(base.ViewModel.LocalVM.Map);
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
		m_TransitionMapLocalViews.EntitiesWithTypes.ForEach(delegate(EnumToObjectSelector<BlueprintMultiEntranceMap, TransitionMapBoardBaseView>.Entity ewt)
		{
			ewt.Value.gameObject.SetActive(value: false);
		});
		TransitionMapBoardBaseView entity = m_TransitionMapLocalViews.GetEntity(base.ViewModel.LocalVM.Map);
		entity.Bind(base.ViewModel.LocalVM);
		m_TransformControls.GoToTarget(entity.Bounds);
		EventBus.Subscribe(this).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(TryClose).AddTo(this);
		Game.Instance.RequestPauseUi(isPaused: true);
		base.gameObject.SetActive(value: true);
	}

	private void TryClose()
	{
		if (!m_GroupChangerOpen && RootVM.Instance?.MessageBoxVM.CurrentValue == null)
		{
			base.ViewModel.Close();
		}
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		if (fullScreenUIType == FullScreenUIType.GroupChanger)
		{
			m_GroupChangerOpen = state;
		}
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_TransitionMapLocalViews.EntitiesWithTypes.ForEach(delegate(EnumToObjectSelector<BlueprintMultiEntranceMap, TransitionMapBoardBaseView>.Entity e)
		{
			e.Value.Unbind();
		});
		m_TransformControls.Reset();
		Game.Instance.RequestPauseUi(isPaused: false);
		m_GroupChangerOpen = false;
		base.gameObject.SetActive(value: false);
	}
}
