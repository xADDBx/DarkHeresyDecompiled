using System.Collections.Generic;
using System.Linq;
using Code.View.UI.Helpers;
using Code.View.UI.UIUtils;
using Kingmaker.Code.Gameplay.Components.Features;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickWeaponHeaderView : BrickBaseView<BrickWeaponHeaderVM>
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
	private RectTransform m_DamageValueContainer;

	[SerializeField]
	private WidgetList m_SpecialTagsContainer;

	[Header("Right Side")]
	[SerializeField]
	private StatDataWidget m_DamageWidget;

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
		List<WeaponTagUISettings> list = base.ViewModel.TagSettings.ToList();
		list.RemoveAll((WeaponTagUISettings t) => t.IsBodyIgnoreTag());
		m_TagsWidgetList.DrawEntries(list.Select((WeaponTagUISettings t) => new WeaponTagData(t, base.ViewModel.BlueprintItem)), m_TagPrefab).AddTo(this);
		m_TagsWidgetList.gameObject.SetActive(list.Any());
		List<SpecialWeaponTagVM> list2 = ((base.ViewModel.Item != null) ? UIUtilityItem.GetSpecialDamageValues(base.ViewModel.Item) : UIUtilityItem.GetSpecialDamageValues(base.ViewModel.BlueprintItem)).Select((KeyValuePair<SpecialWeaponDamageType, string> kvp) => new SpecialWeaponTagVM(kvp.Key, kvp.Value)).ToList();
		m_SpecialTagsContainer.DrawEntries(list2, m_SpecialTagPrefab).AddTo(this);
		m_SpecialTagsContainer.gameObject.SetActive(list2.Any());
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			m_SpecialTagsContainer.Entries.ForEach(delegate(IBindable e)
			{
				(e as SpecialWeaponTagWidget)?.UpdateValueWidth(m_DamageValueContainer.sizeDelta.x);
			});
		}).AddTo(this);
	}
}
