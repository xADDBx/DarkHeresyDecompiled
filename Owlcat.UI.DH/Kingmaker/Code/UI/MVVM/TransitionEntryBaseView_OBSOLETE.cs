using Kingmaker.Globalmap.Blueprints;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TransitionEntryBaseView_OBSOLETE : View<TransitionEntryVM_OBSOLETE>
{
	[SerializeField]
	private BlueprintMultiEntranceEntry.Reference m_EntranceEntry;

	[SerializeField]
	private OwlcatMultiButton m_MapButton;

	public BlueprintMultiEntranceEntry EntranceEntry => m_EntranceEntry;

	public void Initialize()
	{
		m_MapButton.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.ViewModel.IsVisible.CombineLatest(base.ViewModel.IsInteractable, (bool isVisible, bool isInteractable) => new { isVisible, isInteractable }).Subscribe(_ =>
		{
			CheckEntriesEnabled();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_MapButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Enter();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_MapButton.OnConfirmClickAsObservable(), delegate
		{
			base.ViewModel.Enter();
		}).AddTo(this);
		CheckEntriesEnabled();
	}

	private void CheckEntriesEnabled()
	{
		m_MapButton.gameObject.SetActive(base.ViewModel.IsVisible.CurrentValue);
		if (base.ViewModel.IsVisible.CurrentValue)
		{
			m_MapButton.SetInteractable(base.ViewModel.IsInteractable.CurrentValue);
		}
	}
}
