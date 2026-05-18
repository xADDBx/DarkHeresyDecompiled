using Code.View.UI.Helpers;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickExchangeView : BrickBaseView<BrickExchangeVM>
{
	[Header("Elements")]
	[SerializeField]
	private TextValueAddTripleView m_Info;

	[SerializeField]
	private TMP_Text m_ItemType;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private LayoutElement m_IconLayoutElement;

	[SerializeField]
	private TMP_Text m_IconText;

	[Header("Selectables")]
	[SerializeField]
	private OwlcatMultiSelectable m_TextStateSelectable;

	[SerializeField]
	private OwlcatMultiSelectable m_BgrStateSelectable;

	private Color m_DefaultIconColor;

	private float m_DefaultIconSize;

	private void Awake()
	{
		m_DefaultIconColor = m_Icon.color;
		m_DefaultIconSize = m_IconLayoutElement.minWidth;
	}

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_IconText, m_ItemType).AddTo(this);
		}
		base.OnBind();
		m_Info.Bind(base.ViewModel.Data);
		m_ItemType.Or(null)?.SetText(base.ViewModel.ItemType);
		m_Icon.gameObject.SetActive(base.ViewModel.Icon != null);
		m_Icon.sprite = base.ViewModel.Icon;
		if (base.ViewModel.IconColor.HasValue)
		{
			m_Icon.color = base.ViewModel.IconColor.Value;
		}
		m_IconText.Or(null)?.gameObject.SetActive(!string.IsNullOrEmpty(base.ViewModel.IconText));
		m_IconText.Or(null)?.SetText(base.ViewModel.IconText);
		ApplyStyle();
		this.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_Icon.color = m_DefaultIconColor;
		m_IconLayoutElement.minWidth = m_DefaultIconSize;
	}

	private void ApplyStyle()
	{
		m_TextStateSelectable?.SetActiveLayer(base.ViewModel.Type.ToString());
		m_BgrStateSelectable?.SetActiveLayer(base.ViewModel.BackgroundType.ToString());
	}
}
