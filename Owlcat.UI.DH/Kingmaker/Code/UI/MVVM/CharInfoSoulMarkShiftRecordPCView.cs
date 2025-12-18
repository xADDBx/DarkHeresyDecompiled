using Code.View.UI.Helpers;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoSoulMarkShiftRecordPCView : View<CharInfoSoulMarkShiftRecordVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private TextMeshProUGUI m_Description;

	[Header("Colors")]
	[SerializeField]
	private Color m_TorianColor;

	[SerializeField]
	private Color m_XenophiliaColor;

	[SerializeField]
	private Color m_MonodominanceColor;

	[SerializeField]
	private Color m_XanthiteColor;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Description);
		}
		m_Value.text = GetDirectionInfo();
		m_Description.text = base.ViewModel.Description?.Text;
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		m_TextHelper.Dispose();
	}

	private string GetDirectionInfo()
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGBA(GetDirectionColor()) + ">" + $"+{base.ViewModel.Amount} {UIUtilityText.GetSoulMarkDirectionText(base.ViewModel.Axis).Text}</color>";
	}

	private Color GetDirectionColor()
	{
		return base.ViewModel.Axis switch
		{
			AlignmentAxis.Torian => m_TorianColor, 
			AlignmentAxis.Xenophilia => m_XenophiliaColor, 
			AlignmentAxis.Monodominance => m_MonodominanceColor, 
			AlignmentAxis.Xanthite => m_XanthiteColor, 
			_ => Color.magenta, 
		};
	}
}
