using Kingmaker.Blueprints.Root.Strings;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickShotDeviationWithNameView : BrickBaseView<BrickShotDirectionWithNameVM>
{
	[Header("Name")]
	[SerializeField]
	private TextMeshProUGUI m_NameText;

	[Header("Colors")]
	[SerializeField]
	protected Color m_TextOrangeColor;

	[Space]
	[SerializeField]
	private Slider m_DeviationValueSlider;

	[SerializeField]
	private RectTransform m_CentralDeviationAnchorSlider;

	[SerializeField]
	private RectTransform m_SlightDeviationAnchorSlider;

	[SerializeField]
	private RectTransform m_FarDeviationAnchorSlider;

	[SerializeField]
	private TMP_Text m_DeviationMinValueText;

	[SerializeField]
	private TMP_Text m_DeviationMaxValueText;

	[SerializeField]
	private TMP_Text m_DeviationValueText;

	protected override void OnBind()
	{
		string arg = "<color=#" + ColorUtility.ToHtmlStringRGB(m_TextOrangeColor) + "></color>";
		TextMeshProUGUI nameText = m_NameText;
		string text = UIStrings.Instance.CombatLog.ShotDirectionDeviation.Text;
		int shotNumber = base.ViewModel.ShotNumber;
		nameText.text = string.Format(text, shotNumber.ToString(), arg);
		m_NameText.SetTooltip(new TooltipTemplateScatterDeviation(UIStrings.Instance.CombatLog.DeviationHeader.Text, base.ViewModel.DeviationMin, base.ViewModel.DeviationMax, base.ViewModel.DeviationValue, m_TextOrangeColor)).AddTo(this);
		m_CentralDeviationAnchorSlider.anchorMax = new Vector2((float)base.ViewModel.DeviationMin / 100f, 1f);
		m_SlightDeviationAnchorSlider.anchorMin = new Vector2((float)base.ViewModel.DeviationMin / 100f, 1f);
		m_SlightDeviationAnchorSlider.anchorMax = new Vector2((float)base.ViewModel.DeviationMax / 100f, 1f);
		m_FarDeviationAnchorSlider.anchorMin = new Vector2((float)base.ViewModel.DeviationMax / 100f, 1f);
		TMP_Text deviationMinValueText = m_DeviationMinValueText;
		shotNumber = base.ViewModel.DeviationMin;
		deviationMinValueText.text = shotNumber.ToString();
		TMP_Text deviationMaxValueText = m_DeviationMaxValueText;
		shotNumber = base.ViewModel.DeviationMax;
		deviationMaxValueText.text = shotNumber.ToString();
		m_DeviationValueSlider.value = base.ViewModel.DeviationValue;
		TMP_Text deviationValueText = m_DeviationValueText;
		shotNumber = base.ViewModel.DeviationValue;
		deviationValueText.text = shotNumber.ToString();
	}
}
