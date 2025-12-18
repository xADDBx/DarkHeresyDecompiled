using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class UnitBackgroundBlockCommonView : View<UnitBackgroundBlockVM>
{
	[Header("Homeworld")]
	[SerializeField]
	private TextMeshProUGUI m_HomeworldTitle;

	[SerializeField]
	private TextMeshProUGUI m_HomeworldLabel;

	[SerializeField]
	private OwlcatMultiButton m_HomeworldButton;

	[Header("Occupation")]
	[SerializeField]
	private TextMeshProUGUI m_OccupationTitle;

	[SerializeField]
	private TextMeshProUGUI m_OccupationLabel;

	[SerializeField]
	private OwlcatMultiButton m_OccupationButton;

	[Header("MomentOfTriumph")]
	[SerializeField]
	private TextMeshProUGUI m_MomentOfTriumphTitle;

	[SerializeField]
	private TextMeshProUGUI m_MomentOfTriumphLabel;

	[SerializeField]
	private OwlcatMultiButton m_MomentOfTriumphButton;

	[Header("DarkestHour")]
	[SerializeField]
	private TextMeshProUGUI m_DarkestHourTitle;

	[SerializeField]
	private TextMeshProUGUI m_DarkestHourLabel;

	[SerializeField]
	private OwlcatMultiButton m_DarkestHourButton;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		m_TextHelper = new AccessibilityTextHelper(m_HomeworldTitle, m_HomeworldLabel, m_OccupationTitle, m_OccupationLabel, m_MomentOfTriumphTitle, m_MomentOfTriumphLabel, m_DarkestHourTitle, m_DarkestHourLabel).AddTo(this);
		base.ViewModel.Homeworld.Subscribe(delegate(BlueprintFeature f)
		{
			SetBackgroundName(m_HomeworldLabel, f);
		}).AddTo(this);
		base.ViewModel.Occupation.Subscribe(delegate(BlueprintFeature f)
		{
			SetBackgroundName(m_OccupationLabel, f);
		}).AddTo(this);
		base.ViewModel.MomentOfTriumph.Subscribe(delegate(BlueprintFeature f)
		{
			m_MomentOfTriumphButton.gameObject.SetActive(f != null);
			SetBackgroundName(m_MomentOfTriumphLabel, f);
		}).AddTo(this);
		base.ViewModel.DarkestHour.Subscribe(delegate(BlueprintFeature f)
		{
			m_DarkestHourButton.gameObject.SetActive(f != null);
			SetBackgroundName(m_DarkestHourLabel, f);
		}).AddTo(this);
		m_HomeworldTitle.text = UIStrings.Instance.CharGen.Homeworld;
		m_OccupationTitle.text = UIStrings.Instance.CharGen.Occupation;
		m_MomentOfTriumphTitle.text = UIStrings.Instance.CharGen.MomentOfTriumph;
		m_DarkestHourTitle.text = UIStrings.Instance.CharGen.DarkestHour;
		m_HomeworldButton.SetTooltip(base.ViewModel.HomeworldTooltip).AddTo(this);
		m_OccupationButton.SetTooltip(base.ViewModel.OccupationTooltip).AddTo(this);
		m_MomentOfTriumphButton.SetTooltip(base.ViewModel.MomentOfTriumphTooltip).AddTo(this);
		m_DarkestHourButton.SetTooltip(base.ViewModel.DarkestHourTooltip).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}

	private static void SetBackgroundName(TextMeshProUGUI textField, BlueprintFeature feature)
	{
		textField.text = feature?.Name ?? string.Empty;
	}
}
