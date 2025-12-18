using Kingmaker.Blueprints.Root;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class RecommendationMarkerPCView : View<RecommendationMarkerVM>
{
	[SerializeField]
	private Image m_RecommendationImage;

	private bool m_IsInit;

	protected override void OnBind()
	{
		if (base.ViewModel.Recommendation == RecommendationType.Neutral)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		bool flag = base.ViewModel.Recommendation == RecommendationType.Recommended;
		m_RecommendationImage.sprite = (flag ? UIConfig.Instance.UIIcons.Recommended : UIConfig.Instance.UIIcons.NotRecommended);
		m_RecommendationImage.SetGlossaryTooltip(flag ? "RecommendedFeature" : "NotRecommendedFeature").AddTo(this);
	}
}
