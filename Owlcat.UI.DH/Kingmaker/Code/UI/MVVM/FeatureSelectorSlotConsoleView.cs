using Code.View.UI.Helpers;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class FeatureSelectorSlotConsoleView : VirtualListElementViewBase<FeatureSelectorSlotVM>, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate, IConfirmClickHandler
{
	[Header("Icon")]
	[SerializeField]
	protected Image m_Icon;

	[SerializeField]
	protected TextMeshProUGUI m_AcronymText;

	[SerializeField]
	protected OwlcatMultiSelectable m_Button;

	[SerializeField]
	protected TextMeshProUGUI m_DisplayName;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_DisplayName);
		}
		SetupIcon();
		SetupName();
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		m_TextHelper.Dispose();
	}

	public void SetupIcon()
	{
		m_Icon.enabled = true;
		if (base.ViewModel.Icon == null)
		{
			m_AcronymText.gameObject.SetActive(value: true);
			m_AcronymText.text = base.ViewModel.Acronym;
			m_Icon.color = UIUtilityText.GetColorByText(base.ViewModel.Acronym);
			m_Icon.sprite = UIUtilityText.GetIconByText(base.ViewModel.Acronym);
		}
		else
		{
			m_AcronymText.gameObject.SetActive(value: false);
			m_Icon.color = Color.white;
			m_Icon.sprite = base.ViewModel.Icon;
		}
	}

	public void SetupName()
	{
		if (!(m_DisplayName == null))
		{
			m_DisplayName.text = base.ViewModel.DisplayName;
		}
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip.CurrentValue;
	}

	public bool CanConfirmClick()
	{
		return m_Button.IsValid();
	}

	public void OnConfirmClick()
	{
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
