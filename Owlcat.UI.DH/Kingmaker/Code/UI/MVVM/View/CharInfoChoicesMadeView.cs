using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharInfoChoicesMadeView : CharInfoComponentView<CharInfoAlignmentHistoryVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private CharInfoSoulMarkShiftRecordPCView m_SoulMarkShiftRecordView;

	[SerializeField]
	private TextMeshProUGUI m_Biography;

	private AccessibilityTextHelper m_TextHelper;

	public override void Initialize()
	{
		base.Initialize();
		m_TextHelper = new AccessibilityTextHelper(m_Label);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_TextHelper.Dispose();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		m_Label.text = (base.ViewModel.IsMainPlayer ? UIStrings.Instance.CharacterSheet.Decisions : UIStrings.Instance.CharacterSheet.Biography);
		m_Biography.gameObject.SetActive(!base.ViewModel.IsMainPlayer);
		m_Biography.text = base.ViewModel.Biography;
		if (!base.ViewModel.IsMainPlayer)
		{
			m_WidgetList.Clear();
		}
		else
		{
			DrawEntities();
		}
		m_TextHelper.UpdateTextSize();
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.SoulMarkShiftsHistory, m_SoulMarkShiftRecordView);
	}
}
