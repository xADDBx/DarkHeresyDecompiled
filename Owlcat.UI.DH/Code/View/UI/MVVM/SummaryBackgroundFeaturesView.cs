using Kingmaker.Code.UI.MVVM;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Code.View.UI.MVVM;

public class SummaryBackgroundFeaturesView : View<SummaryBackgroundFeaturesVM>
{
	[SerializeField]
	private TooltipPlaces m_TooltipPlace;

	[Header("Views")]
	[SerializeField]
	private BackgroundFeatureView[] m_FeatureViews;

	protected override void OnBind()
	{
		base.ViewModel.Features.ObserveCountChanged().Subscribe(delegate
		{
			BindFeatureViews();
		}).AddTo(this);
		BindFeatureViews();
	}

	private void BindFeatureViews()
	{
		for (int i = 0; i < m_FeatureViews.Length; i++)
		{
			BackgroundFeatureVM backgroundFeatureVM = ((i < base.ViewModel.Features.Count) ? base.ViewModel.Features[i] : null);
			if (backgroundFeatureVM != null)
			{
				m_FeatureViews[i].Bind(backgroundFeatureVM);
				m_FeatureViews[i].SetupTooltip(m_TooltipPlace.GetMainTooltipConfig());
			}
		}
	}
}
