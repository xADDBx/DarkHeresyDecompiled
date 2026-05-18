using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickIconStatValueView : BrickBaseView<BrickIconStatValueVM>
{
	[Header("Elements")]
	[SerializeField]
	private TextValueAddTripleView m_Info;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TMP_Text m_IconText;

	[SerializeField]
	private LayoutElement m_IconLayoutElement;

	[SerializeField]
	private Image m_Background;

	[Header("Selectables")]
	[SerializeField]
	private OwlcatMultiSelectable m_HasValueSelectable;

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
		base.OnBind();
		m_Info.Bind(base.ViewModel.Info);
		m_Icon.gameObject.SetActive(base.ViewModel.Icon != null);
		m_Icon.sprite = base.ViewModel.Icon;
		if (base.ViewModel.IconColor.HasValue)
		{
			m_Icon.color = base.ViewModel.IconColor.Value;
		}
		SetIconSize(base.ViewModel.IconSize);
		m_IconText.Or(null)?.gameObject.SetActive(!string.IsNullOrEmpty(base.ViewModel.IconText));
		m_IconText.Or(null)?.SetText(base.ViewModel.IconText);
		m_HasValueSelectable.SetActiveLayer(base.ViewModel.HasValue ? "HasValues" : "NoValues");
		ApplyStyle();
		this.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		base.ViewModel.ReactiveValue?.Subscribe(m_Info.UpdateValue).AddTo(this);
		base.ViewModel.ReactiveAddValue?.Subscribe(m_Info.UpdateAddValue).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_Icon.color = m_DefaultIconColor;
		m_IconLayoutElement.minWidth = m_DefaultIconSize;
	}

	private void ApplyStyle()
	{
		m_TextStateSelectable.SetActiveLayer(base.ViewModel.Type.ToString());
		m_BgrStateSelectable?.SetActiveLayer(base.ViewModel.BackgroundType.ToString());
	}

	private void SetIconSize(float? width)
	{
		if ((bool)m_IconLayoutElement && width.HasValue)
		{
			m_IconLayoutElement.minWidth = width.Value;
		}
	}
}
