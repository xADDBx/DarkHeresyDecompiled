using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharInfoCurrentCareerView : View<CareerPathVM>, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private TextMeshProUGUI m_CareerLabel;

	[SerializeField]
	private TextMeshProUGUI m_CareerName;

	[SerializeField]
	private TextMeshProUGUI m_CareerValue;

	[SerializeField]
	private Image m_Icon;

	protected override void OnBind()
	{
		m_CareerLabel.text = UIStrings.Instance.CharacterSheet.Career;
		m_CareerName.text = base.ViewModel.Name;
		TextMeshProUGUI careerValue = m_CareerValue;
		string text = base.ViewModel.CurrentRank.CurrentValue.ToString();
		int maxRank = base.ViewModel.MaxRank;
		careerValue.text = text + "/" + maxRank;
		m_Icon.sprite = base.ViewModel.Icon.CurrentValue;
		this.SetTooltip(base.ViewModel.CareerTooltip).AddTo(this);
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.CareerTooltip;
	}
}
