using Code.View.UI.Helpers;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickItemRestrictionView : BrickBaseView<BrickItemRestrictionVM>
{
	[Header("Elements")]
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private ItemRestrictionWidget ItemRestrictionWidgetPrefab;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	[SerializeField]
	private TextMeshProUGUI m_OwnerNameLabel;

	[Header("Values")]
	[SerializeField]
	private TextStyle m_Style;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_OwnerNameLabel).AddTo(this);
		}
		m_StateSelectable.SetActiveLayer(GetStateLayer());
		m_WidgetList.DrawEntries(base.ViewModel.GetFalseRestrictionStrings(), ItemRestrictionWidgetPrefab);
		SetOwnerName(base.ViewModel.HasOwnerName ? base.ViewModel.OwnerName : string.Empty);
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		m_WidgetList.Clear();
		SetOwnerName(string.Empty);
	}

	private void SetOwnerName(string value)
	{
		m_OwnerNameLabel.text = (string.IsNullOrEmpty(value) ? string.Empty : value.ApplyStyle(m_Style));
	}

	private string GetStateLayer()
	{
		if (!base.ViewModel.CanEquipItem)
		{
			return "Default";
		}
		if (!base.ViewModel.HasFalseRestriction)
		{
			return "Positive";
		}
		return "Negative";
	}
}
