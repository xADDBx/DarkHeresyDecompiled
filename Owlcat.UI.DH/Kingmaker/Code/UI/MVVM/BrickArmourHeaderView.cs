using System.Linq;
using Code.View.UI.Helpers;
using Kingmaker.Code.Gameplay.Components.Features;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickArmourHeaderView : BrickBaseView<BrickArmourHeaderVM>
{
	[Header("Title")]
	[SerializeField]
	private TMP_Text m_MainTitle;

	[Header("Left Side")]
	[SerializeField]
	private TMP_Text m_ItemType;

	[SerializeField]
	private TMP_Text m_ItemLabel;

	[SerializeField]
	private StatDataWidget m_ArmourDurabilityWidget;

	[SerializeField]
	private StatDataWidget m_DamageReductionWidget;

	[SerializeField]
	private StatDataWidget m_DefenceWidget;

	[Header("Right Side")]
	[SerializeField]
	private GameObject m_IconContainer;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private GameObject m_UpgradeItemIndicator;

	[SerializeField]
	private WidgetList m_TagsWidgetList;

	[Header("Views")]
	[SerializeField]
	private TagWidget m_TagPrefab;

	protected override void OnBind()
	{
		m_TextHelper = new AccessibilityTextHelper(m_MainTitle, m_ItemType, m_ItemLabel).AddTo(this);
		base.OnBind();
		m_MainTitle.text = base.ViewModel.MainTitle;
		m_IconContainer.SetActive(base.ViewModel.Image != null);
		m_Icon.sprite = base.ViewModel.Image;
		m_ArmourDurabilityWidget.Bind(base.ViewModel.ArmourDurability);
		m_DamageReductionWidget.Bind(base.ViewModel.DamageReduction);
		if (m_DefenceWidget != null)
		{
			if (base.ViewModel.ArmourDefence != null)
			{
				m_DefenceWidget.Bind(base.ViewModel.ArmourDefence);
			}
			else
			{
				m_DefenceWidget.gameObject.SetActive(value: false);
			}
		}
		SetText(m_ItemType, base.ViewModel.ItemType);
		SetText(m_ItemLabel, base.ViewModel.ItemLabel);
		m_TagsWidgetList.DrawEntries(base.ViewModel.TagSettings.Select((ArmourTagUISettings t) => new ArmourTagData(t, base.ViewModel.BlueprintItem)), m_TagPrefab).AddTo(this);
		m_TagsWidgetList.gameObject.SetActive(base.ViewModel.TagSettings.Any());
		if ((bool)m_UpgradeItemIndicator)
		{
			m_UpgradeItemIndicator.SetActive(base.ViewModel.HasUpgrade);
		}
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_ArmourDurabilityWidget.Unbind();
		m_DamageReductionWidget.Unbind();
		if (m_DefenceWidget != null && base.ViewModel.ArmourDefence != null)
		{
			m_DefenceWidget.Unbind();
		}
		m_TextHelper = null;
		m_TagsWidgetList.Clear();
	}

	private void SetText(TMP_Text textField, string text)
	{
		textField.gameObject.SetActive(!string.IsNullOrEmpty(text));
		textField.text = text;
	}
}
