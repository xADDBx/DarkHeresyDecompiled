using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class SoulMarkRewardBaseView : View<AlignmentMarkRewardVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_FeatureName;

	[SerializeField]
	protected OwlcatMultiButton m_MainButton;

	protected AccessibilityTextHelper TextHelper;

	protected override void OnBind()
	{
		TextHelper = new AccessibilityTextHelper(m_Title, m_FeatureName).AddTo(this);
		m_Title.text = UIStrings.Instance.PopUps.SoulMarkRewardTitle;
		m_FeatureName.text = base.ViewModel.FeatureName;
		m_Icon.sprite = base.ViewModel.FeatureIcon;
		TextHelper.UpdateTextSize();
	}
}
