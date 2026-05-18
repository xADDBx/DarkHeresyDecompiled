using System;
using System.Collections.Generic;
using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickBuffView : BrickBaseView<BrickBuffVM>
{
	[Header("Elements")]
	[SerializeField]
	protected TMP_Text m_Label;

	[SerializeField]
	protected GameObject m_Background;

	[SerializeField]
	private HorizontalLayoutGroup m_HorizontalLayoutGroup;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	protected GameObject m_IconBlock;

	[SerializeField]
	private TMP_Text m_Duration;

	[Header("Acronym")]
	[SerializeField]
	private TMP_Text m_Acronym;

	[SerializeField]
	private TalentGroupView m_TalentGroupView;

	[Header("Hidden")]
	[SerializeField]
	private HiddenPartWidget m_HiddenPartWidget;

	[Header("SourceBlock")]
	[SerializeField]
	private GameObject m_SourcePanel;

	[SerializeField]
	private TMP_Text m_SourceName;

	[SerializeField]
	private TMP_Text m_StackText;

	[Header("DOTBlock")]
	[SerializeField]
	private GameObject m_DOTPanel;

	[SerializeField]
	private TMP_Text m_DOTDescription;

	[SerializeField]
	private TMP_Text m_DOTDamage;

	private IDisposable m_TooltipDisposable;

	protected override void OnBind()
	{
		base.OnBind();
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Label).AddTo(this);
		}
		m_Label.text = base.ViewModel.Name;
		bool flag = (bool)base.ViewModel.Icon || base.ViewModel.Acronym != null;
		m_IconBlock.SetActive(flag);
		if (flag)
		{
			m_Icon.sprite = base.ViewModel.Icon;
			m_Icon.color = base.ViewModel.IconColor;
			m_HorizontalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
		}
		else
		{
			m_HorizontalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
		}
		m_Acronym.text = base.ViewModel.Acronym;
		m_Acronym.color = UIConfig.Instance.TooltipsConfig.GetAcronymColor(base.ViewModel.TalentIconsInfo?.HasGroups);
		m_Background?.SetActive(base.ViewModel.AvailableBackground);
		m_TalentGroupView.Or(null)?.SetupView(base.ViewModel.TalentIconsInfo);
		m_HiddenPartWidget.SetHiddenStateImmediate(base.ViewModel.IsHidden.CurrentValue);
		base.ViewModel.IsHidden.Skip(1).Subscribe(m_HiddenPartWidget.SetHiddenState).AddTo(this);
		base.ViewModel.IsHidden.Subscribe(delegate
		{
			UpdateTooltip();
		}).AddTo(this);
		m_TextHelper.UpdateTextSize();
		base.ViewModel.Duration.Subscribe(delegate(string t)
		{
			m_Duration.text = t;
			if (t == string.Empty)
			{
				base.gameObject.SetActive(value: false);
			}
		}).AddTo(this);
		m_SourcePanel.Or(null)?.SetActive(!string.IsNullOrWhiteSpace(base.ViewModel.SourceName));
		if (m_SourceName != null)
		{
			m_SourceName.text = base.ViewModel.SourceName;
		}
		if (m_StackText != null)
		{
			m_StackText.text = base.ViewModel.Stack;
		}
		m_DOTPanel.SetActive(base.ViewModel.IsDOT);
		if (base.ViewModel.IsDOT)
		{
			m_DOTDescription.text = base.ViewModel.DOTDesc;
			m_DOTDamage.text = base.ViewModel.DOTDamage;
		}
	}

	private void UpdateTooltip()
	{
		m_TooltipDisposable?.Dispose();
		m_TooltipDisposable = this.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig
		{
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f),
				new Vector2(0f, 0.5f)
			}
		}).AddTo(this);
	}
}
