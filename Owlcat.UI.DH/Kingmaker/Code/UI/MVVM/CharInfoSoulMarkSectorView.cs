using System.Collections.Generic;
using System.Linq;
using Code.View.UI.Helpers;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoSoulMarkSectorView : CharInfoComponentView<CharInfoAlignmentMarksSectorVM>
{
	[FormerlySerializedAs("m_SectorLabel")]
	[SerializeField]
	private OwlcatMultiButton m_SectorInfo;

	[Header("Texts")]
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private TextMeshProUGUI m_Level;

	[Header("Slots")]
	[SerializeField]
	private CharInfoAlignmentAbilitySlotPCView[] m_AbilitySlots;

	[SerializeField]
	private float[] m_GlobalProgressThreshold;

	private TooltipHandler m_TooltipHandler;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Title, m_Value, m_Level);
		}
		base.OnBind();
		m_Title.text = UIUtilityText.GetSoulMarkDirectionText(base.ViewModel.Direction);
		m_TextHelper.UpdateTextSize();
		RefreshTooltip();
		BindSlots();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_AbilitySlots.ForEach(delegate(CharInfoAlignmentAbilitySlotPCView s)
		{
			s.Unbind();
		});
		m_TextHelper.Dispose();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		SetupSector();
		RefreshTooltip();
	}

	private void RefreshTooltip()
	{
		m_TooltipHandler?.Dispose();
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

	private void SetupSector()
	{
		int currentLevel = base.ViewModel.CurrentLevel;
		m_Level.text = UIUtilityText.ArabicToRoman(Mathf.Min(base.ViewModel.CurrentLevel, base.ViewModel.AbilitySlotVms.Count));
		int num = ((currentLevel + 1 < base.ViewModel.RankThresholds.Count) ? base.ViewModel.RankThresholds[currentLevel + 1] : base.ViewModel.MaxRank);
		m_Value.text = base.ViewModel.CurrentRank + "/" + num;
		if (currentLevel + 1 < base.ViewModel.RankThresholds.Count && currentLevel + 1 < m_GlobalProgressThreshold.Length)
		{
			float t = 1f * (float)(base.ViewModel.CurrentRank - base.ViewModel.RankThresholds[currentLevel]) / (float)(base.ViewModel.RankThresholds[currentLevel + 1] - base.ViewModel.RankThresholds[currentLevel]);
			float globalProgress = Mathf.Lerp(m_GlobalProgressThreshold[currentLevel], m_GlobalProgressThreshold[currentLevel + 1], t);
			SetGlobalProgress(globalProgress);
		}
		else
		{
			SetGlobalProgress(1f);
		}
	}

	private void SetGlobalProgress(float amount)
	{
	}

	public List<SimpleConsoleNavigationEntity> GetEntities()
	{
		List<SimpleConsoleNavigationEntity> list = m_AbilitySlots.Select((CharInfoAlignmentAbilitySlotPCView s) => s.NavigationEntity).ToList();
		list.Add(new SimpleConsoleNavigationEntity(m_SectorInfo, base.ViewModel.Tooltip));
		return list;
	}
}
