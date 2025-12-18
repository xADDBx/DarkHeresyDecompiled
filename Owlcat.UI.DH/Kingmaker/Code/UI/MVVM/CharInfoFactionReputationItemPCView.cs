using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoFactionReputationItemPCView : View<CharInfoFactionReputationItemVM>
{
	[SerializeField]
	private Image m_FactionImage;

	[SerializeField]
	private Image m_InfoButton;

	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_FactionLabel;

	[SerializeField]
	private TextMeshProUGUI m_ReputationLabel;

	[Header("Progress")]
	[SerializeField]
	private TextMeshProUGUI m_ReputationProgressValue;

	[SerializeField]
	private Image m_ReputationProgressBar;

	[SerializeField]
	private TextMeshProUGUI m_ReputationLevelRomeNumber;

	[SerializeField]
	private Color m_NormalColor;

	[SerializeField]
	private Color m_MaxLevelColor;

	[Header("Vendors Location Info")]
	[SerializeField]
	private TextMeshProUGUI m_VendorsLabel;

	[SerializeField]
	private TextMeshProUGUI m_FactionDescription;

	[SerializeField]
	private Image m_Background;

	[SerializeField]
	protected WidgetList m_WidgetList;

	[SerializeField]
	private FactionVendorInformationBaseView m_FactionVendorInformationPCViewPrefab;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_FactionLabel, m_ReputationLabel, m_ReputationProgressValue, m_VendorsLabel, m_FactionDescription);
		}
		m_FactionImage.sprite = null;
		m_FactionLabel.text = base.ViewModel.Label;
		if ((bool)m_ReputationLabel)
		{
			m_ReputationLabel.text = UIStrings.Instance.CharacterSheet.FactionsReputation;
		}
		if ((bool)m_VendorsLabel)
		{
			m_VendorsLabel.text = UIStrings.Instance.CharacterSheet.Vendors;
		}
		if ((bool)m_FactionDescription)
		{
			m_FactionDescription.text = base.ViewModel.Description;
		}
		DrawReputation();
		base.ViewModel.CurrentReputation.Subscribe(delegate
		{
			DrawReputation();
		}).AddTo(this);
		base.ViewModel.ReputationLevel.Subscribe(delegate(int level)
		{
			SetVendorReputationLevel(level);
		}).AddTo(this);
		base.ViewModel.IsMaxLevel.Subscribe(SetColorMaxLevel).AddTo(this);
		if ((bool)m_InfoButton)
		{
			m_InfoButton.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		}
		else if ((bool)m_Background)
		{
			m_Background.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		}
		DrawVendors();
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		m_TextHelper.Dispose();
	}

	private void DrawVendors()
	{
		if (!(m_WidgetList == null))
		{
			m_WidgetList.Clear();
			FactionVendorInformationVM[] datas = base.ViewModel.Vendors.ToArray();
			m_WidgetList.DrawEntries(datas, m_FactionVendorInformationPCViewPrefab);
		}
	}

	private void SetColorMaxLevel(bool max)
	{
		m_ReputationProgressBar.color = (max ? m_MaxLevelColor : m_NormalColor);
		m_ReputationProgressValue.color = (max ? m_MaxLevelColor : m_NormalColor);
		m_ReputationLevelRomeNumber.color = (max ? m_MaxLevelColor : m_NormalColor);
	}

	private void DrawReputation()
	{
		m_ReputationProgressValue.text = base.ViewModel.GetCurrentAndNextLevelProgress();
		m_ReputationProgressBar.fillAmount = base.ViewModel.GetNextLevelReputationPoints() / (float)base.ViewModel.GetCurrentReputationPoints();
	}

	private void SetVendorReputationLevel(int level)
	{
		m_ReputationLevelRomeNumber.text = level.ToString();
	}
}
