using Code.View.UI.Helpers;
using Owlcat.Plugins.DotNetExtensions;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickLevelUpFeatureView : BrickBaseView<BrickLevelUpFeatureVM>
{
	[Header("Elements")]
	[SerializeField]
	private TextValueTupleView m_Title;

	[SerializeField]
	private TextValueTupleView m_Subtitle;

	[SerializeField]
	private TMP_Text m_Acronym;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private OwlcatMultiSelectable m_IconDecorSelectable;

	[SerializeField]
	private TalentGroupView m_TalentGroupView;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Acronym).AddTo(this);
		}
		base.OnBind();
		m_Title.Bind(base.ViewModel.UIData.Title);
		m_Subtitle.Bind(base.ViewModel.UIData.Subtitle);
		m_Acronym.transform.parent.gameObject.SetActive(!base.ViewModel.UIData.Acronym.IsNullOrEmpty());
		m_Acronym.SetText(base.ViewModel.UIData.Acronym);
		m_Icon.sprite = base.ViewModel.UIData.Icon;
		m_Icon.gameObject.SetActive(base.ViewModel.UIData.Icon != null);
		m_IconDecorSelectable.SetActiveLayer(base.ViewModel.UIData.IconDecor.ToString());
		m_TalentGroupView.SetupView(base.ViewModel.UIData.TalentIconInfo);
		m_TalentGroupView.gameObject.SetActive(base.ViewModel.UIData.TalentIconInfo != null);
		this.SetTooltip(base.ViewModel.UIData.Tooltip).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}
}
