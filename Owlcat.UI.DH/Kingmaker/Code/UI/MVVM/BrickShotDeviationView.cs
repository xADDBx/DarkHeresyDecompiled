using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickShotDeviationView : BrickBaseView<BrickShotDirectionVM>
{
	[Header("Elements")]
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
		m_CentralDeviationAnchorSlider.anchorMax = new Vector2((float)base.ViewModel.DeviationMin / 100f, 1f);
		m_SlightDeviationAnchorSlider.anchorMin = new Vector2((float)base.ViewModel.DeviationMin / 100f, 1f);
		m_SlightDeviationAnchorSlider.anchorMax = new Vector2((float)base.ViewModel.DeviationMax / 100f, 1f);
		m_FarDeviationAnchorSlider.anchorMin = new Vector2((float)base.ViewModel.DeviationMax / 100f, 1f);
		TMP_Text deviationMinValueText = m_DeviationMinValueText;
		int deviationMin = base.ViewModel.DeviationMin;
		deviationMinValueText.text = deviationMin.ToString();
		TMP_Text deviationMaxValueText = m_DeviationMaxValueText;
		deviationMin = base.ViewModel.DeviationMax;
		deviationMaxValueText.text = deviationMin.ToString();
		m_DeviationValueSlider.value = base.ViewModel.DeviationValue;
		TMP_Text deviationValueText = m_DeviationValueText;
		deviationMin = base.ViewModel.DeviationValue;
		deviationValueText.text = deviationMin.ToString();
	}
}
