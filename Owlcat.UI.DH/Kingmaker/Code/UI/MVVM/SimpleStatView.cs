using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SimpleStatView : View<CharInfoStatVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_StatName;

	[SerializeField]
	private TMP_Text m_StatValue;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_StatName, m_StatValue).AddTo(this);
		}
		m_StatName.text = base.ViewModel.Name.CurrentValue;
		m_StatValue.text = base.ViewModel.StatValue.ToString();
		base.ViewModel.StatValue.CombineLatest(base.ViewModel.PreviewStatValue, base.ViewModel.Bonus, (int stat, int previewStat, int bonus) => new { stat, previewStat, bonus }).Subscribe(_ =>
		{
			SetValue();
		}).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}

	private void SetValue()
	{
		if (!base.ViewModel.IsValueEnabled.CurrentValue)
		{
			m_StatValue.text = "-";
		}
		else
		{
			m_StatValue.text = base.ViewModel.StatValue.ToString();
		}
	}
}
