using System.Collections.Generic;
using System.Linq;
using Code.View.UI.Helpers;
using Kingmaker.Code.Gameplay.Components.Features;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.View.UI.MVVM.Tooltip.Bricks.Items;

public class TooltipBrickWeaponHeaderView : View<TooltipBrickWeaponHeaderVM>
{
	[Header("Title")]
	[SerializeField]
	private TextMeshProUGUI m_MainTitle;

	[Header("Left Side")]
	[SerializeField]
	private TMP_Text m_ItemType;

	[SerializeField]
	private TMP_Text m_ItemLabel;

	[SerializeField]
	private RectTransform m_DamageValueContainer;

	[SerializeField]
	private WidgetList m_SpecialTagsContainer;

	[Header("Right Side")]
	[SerializeField]
	private StatWidget m_DamageWidget;

	[SerializeField]
	private GameObject m_IconContainer;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TMP_Text m_ItemSubtitle;

	[SerializeField]
	private GameObject m_UpgradeItemIndicator;

	[SerializeField]
	private WidgetList m_TagsWidgetList;

	[Header("Views")]
	[SerializeField]
	private TagWidget m_TagPrefab;

	[SerializeField]
	private SpecialWeaponTagWidget m_SpecialTagPrefab;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		m_TextHelper = new AccessibilityTextHelper(m_MainTitle, m_ItemType, m_ItemLabel, m_ItemSubtitle).AddTo(this);
		base.OnBind();
		m_MainTitle.text = base.ViewModel.MainTitle;
		m_IconContainer.SetActive(base.ViewModel.Image != null);
		m_Icon.sprite = base.ViewModel.Image;
		SetText(m_ItemType, base.ViewModel.ItemType);
		SetText(m_ItemLabel, base.ViewModel.ItemLabel);
		SetText(m_ItemSubtitle, base.ViewModel.ItemSubtitle);
		m_DamageWidget.Bind(base.ViewModel.DamageValue);
		SetupTags();
		if ((bool)m_UpgradeItemIndicator)
		{
			m_UpgradeItemIndicator.SetActive(base.ViewModel.HasUpgrade);
		}
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_TextHelper = null;
		m_TagsWidgetList.Clear();
	}

	private void SetText(TMP_Text textField, string text)
	{
		textField.gameObject.SetActive(!string.IsNullOrEmpty(text));
		textField.text = text;
	}

	private void SetupTags()
	{
		IEnumerable<WeaponTagUISettings> enumerable = base.ViewModel.TagSettings.Where((WeaponTagUISettings t) => TooltipBrickWeaponHeaderVM.SpecialTags.Contains(t.Tag));
		IEnumerable<WeaponTagUISettings> source = base.ViewModel.TagSettings.Except(enumerable);
		m_TagsWidgetList.DrawEntries(source.Select((WeaponTagUISettings t) => new WeaponTagData(t)), m_TagPrefab).AddTo(this);
		m_TagsWidgetList.gameObject.SetActive(base.ViewModel.TagSettings.Any());
		m_SpecialTagsContainer.DrawEntries(enumerable.Select((WeaponTagUISettings t) => (base.ViewModel.SpecialTagsValues.GetValueOrDefault(t.Tag), t)), m_SpecialTagPrefab).AddTo(this);
		m_SpecialTagsContainer.gameObject.SetActive(enumerable.Any());
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			m_SpecialTagsContainer.Entries.ForEach(delegate(IBindable e)
			{
				(e as SpecialWeaponTagWidget)?.UpdateValueWidth(m_DamageValueContainer.sizeDelta.x);
			});
		}).AddTo(this);
	}
}
