using Code.View.UI.Helpers;
using Code.View.UI.UIUtils;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoAlignmentGroupView : CharInfoComponentView<CharInfoAlignmentGroupVM>
{
	[Header("Elements")]
	[FormerlySerializedAs("m_SectorInfo")]
	[SerializeField]
	private OwlcatMultiButton m_GroupInfo;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private TextMeshProUGUI m_Level;

	[Header("Slots")]
	[SerializeField]
	private CharInfoAlignmentAbilitySlotBaseView[] m_AbilitySlots;

	private TooltipHandler m_TooltipHandler;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Title, m_Value, m_Level);
		}
		base.OnBind();
		m_Title.text = UIUtilityAlignment.GetAlignmentDirectionText(base.ViewModel.Direction);
		m_TextHelper.UpdateTextSize();
		RefreshTooltip();
		BindSlots();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_AbilitySlots.ForEach(delegate(CharInfoAlignmentAbilitySlotBaseView s)
		{
			s.Unbind();
		});
		m_TextHelper.Dispose();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		SetupGroup();
		RefreshTooltip();
	}

	private void RefreshTooltip()
	{
		m_TooltipHandler?.Dispose();
		m_TooltipHandler = m_GroupInfo.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
	}

	private void BindSlots()
	{
		int num = Mathf.Min(m_AbilitySlots.Length, base.ViewModel.AbilitySlotVms.Count);
		for (int i = 0; i < num; i++)
		{
			m_AbilitySlots[i].BindSection(base.ViewModel.AbilitySlotVms[i]);
		}
		for (int j = num; j < m_AbilitySlots.Length; j++)
		{
			m_AbilitySlots[j].gameObject.SetActive(value: false);
		}
	}

	private void SetupGroup()
	{
		m_Level.text = UIUtilityText.ArabicToRoman(Mathf.Min(base.ViewModel.CurrentLevel, base.ViewModel.AbilitySlotVms.Count));
		m_Value.text = base.ViewModel.CurrentRank + "/" + base.ViewModel.MaxRank;
	}
}
