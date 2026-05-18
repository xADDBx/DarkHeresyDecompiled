using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickFeatureShortDescriptionView : BrickBaseView<BrickFeatureDescriptionVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Label;

	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextWithParent m_Acronym;

	protected override void OnBind()
	{
		base.OnBind();
		m_Label.text = base.ViewModel.Name;
		m_Description.text = base.ViewModel.Description;
		m_Icon.sprite = base.ViewModel.Icon;
		m_Icon.color = base.ViewModel.IconColor;
		m_Acronym.Container.gameObject.SetActive(base.ViewModel.Icon == null);
		m_Acronym.Text.SetText(base.ViewModel.Acronym);
		m_Description.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)).AddTo(this);
		this.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
	}
}
