using Code.View.UI.MVVM;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenSummaryPhaseDetailedView : CharGenPhaseDetailedView<CharGenSummaryPhaseVM>
{
	[Header("Character Info")]
	[SerializeField]
	private CharGenNameBaseView m_CharGenNameView;

	[SerializeField]
	private SummaryBackgroundFeaturesView m_BackgroundFeaturesView;

	[SerializeField]
	private OwlcatMultiButton m_TogglePlaningModeButton;

	protected override bool HasYScrollBindInternal => false;

	protected override void OnBind()
	{
		base.OnBind();
		m_CharGenNameView.Bind(base.ViewModel.CharGenNameVM);
		m_BackgroundFeaturesView.Bind(base.ViewModel.BackgroundFeaturesVM);
		ObservableSubscribeExtensions.Subscribe(m_TogglePlaningModeButton.OnLeftClickAsObservable(), delegate
		{
			RootVM.Instance.CharGenContext.CharGenVM?.CurrentValue.TogglePlaningMode();
		}).AddTo(this);
	}
}
