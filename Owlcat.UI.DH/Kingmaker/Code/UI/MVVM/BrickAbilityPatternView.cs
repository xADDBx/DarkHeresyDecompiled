using Code.View.UI.Helpers;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Pathfinding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityPatternView : BrickBaseView<BrickAbilityPatternVM>
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private GameObject m_IconBlock;

	[SerializeField]
	private AbilityPatternView m_PatternView;

	[SerializeField]
	private GameObject m_PatternBlock;

	[SerializeField]
	private TMP_Text m_Description;

	protected override void OnBind()
	{
		base.OnBind();
		m_TextHelper = new AccessibilityTextHelper(m_Description);
		SetupIcon();
		SetupPattern();
		m_Description.SetText(base.ViewModel.Description);
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		m_TextHelper?.Dispose();
		m_TextHelper = null;
		m_PatternView.Destroy();
	}

	private void SetupIcon()
	{
		bool flag = base.ViewModel.Icon != null;
		m_IconBlock.SetActive(flag);
		if (flag)
		{
			m_Icon.sprite = base.ViewModel.Icon;
		}
	}

	private void SetupPattern()
	{
		UIUtilityItem.UIPatternData patternData = base.ViewModel.PatternData;
		int num;
		if (patternData != null)
		{
			PatternGridData patternCells = patternData.PatternCells;
			num = ((patternCells.Bounds.Area > 1) ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool flag = (byte)num != 0;
		m_PatternBlock.SetActive(flag);
		if (flag)
		{
			m_PatternView.Initialize(base.ViewModel.PatternData);
		}
	}
}
