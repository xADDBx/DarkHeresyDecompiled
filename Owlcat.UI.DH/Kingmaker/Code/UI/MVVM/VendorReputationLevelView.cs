using DG.Tweening;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class VendorReputationLevelView : VirtualListElementViewBase<VendorReputationLevelVM>
{
	[SerializeField]
	private TextMeshProUGUI m_ReputationPointsLabel;

	[SerializeField]
	private TextMeshProUGUI m_LevelLabel;

	[SerializeField]
	protected GameObject m_LockedObject;

	[SerializeField]
	protected OwlcatMultiButton m_Button;

	[SerializeField]
	private DOTweenAnimation m_Highlight;

	protected override void BindViewImplementation()
	{
		m_ReputationPointsLabel.text = base.ViewModel.ReputationPoints.ToString();
		m_LevelLabel.text = base.ViewModel.ReputationLevel.ToString();
		m_LockedObject.SetActive(base.ViewModel.Locked);
		m_Button.SetActiveLayer(base.ViewModel.Locked ? 1 : 0);
		AddDisposable(ObservableSubscribeExtensions.Subscribe(base.ViewModel.OnHighlight, delegate
		{
			m_Highlight.DOPlay();
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
