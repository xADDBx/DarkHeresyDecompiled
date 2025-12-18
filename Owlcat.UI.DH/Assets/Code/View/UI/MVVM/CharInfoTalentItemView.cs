using Kingmaker.Code.UI.MVVM;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Assets.Code.View.UI.MVVM;

public class CharInfoTalentItemView : View<CharInfoTalentItemVM>
{
	[SerializeField]
	private TextMeshProUGUI m_TalentName;

	[SerializeField]
	private TextMeshProUGUI m_TalentAcronym;

	[SerializeField]
	private OwlcatSelectable m_Selectable;

	[SerializeField]
	private TalentGroupView m_TalentIconView;

	protected override void OnBind()
	{
		base.OnBind();
		m_TalentName.text = base.ViewModel.Name;
		m_TalentAcronym.text = base.ViewModel.Acronym;
		base.ViewModel.IsVisible.Subscribe(delegate(bool isVisible)
		{
			base.gameObject.SetActive(isVisible);
		}).AddTo(this);
		m_Selectable.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		m_TalentIconView.SetupView(base.ViewModel.TalentInfo);
	}
}
