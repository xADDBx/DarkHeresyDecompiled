using Code.View.UI.Helpers;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoStatPCView : View<CharInfoStatVM>
{
	[Header("Labels")]
	[SerializeField]
	private TMP_Text m_LongName;

	[SerializeField]
	private TMP_Text m_ShortName;

	[Header("Values")]
	[SerializeField]
	private TMP_Text m_Value;

	[SerializeField]
	private bool m_AlwaysShowSign;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_LongName, m_ShortName, m_Value).AddTo(this);
		}
		SetLabels();
		base.ViewModel.IsValueEnabled.Subscribe(delegate
		{
			SetValue();
		}).AddTo(this);
		base.ViewModel.StatValue.Subscribe(delegate
		{
			SetValue();
		}).AddTo(this);
		base.ViewModel.StringValue.Subscribe(delegate
		{
			SetValue();
		}).AddTo(this);
		SetTooltip();
		m_TextHelper.UpdateTextSize();
	}

	private void SetLabels()
	{
		m_LongName.Or(null)?.SetText(base.ViewModel.Name.CurrentValue);
		m_ShortName.Or(null)?.SetText(base.ViewModel.ShortName);
	}

	private void SetValue()
	{
		if (!base.ViewModel.IsValueEnabled.CurrentValue)
		{
			m_Value.text = "-";
		}
		else if (string.IsNullOrEmpty(base.ViewModel.StringValue.CurrentValue))
		{
			m_Value.text = (m_AlwaysShowSign ? UIUtilityText.AddSign(base.ViewModel.StatValue.CurrentValue) : base.ViewModel.StatValue.ToString());
		}
		else
		{
			m_Value.text = base.ViewModel.StringValue.CurrentValue;
		}
	}

	private void SetTooltip()
	{
		this.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
	}
}
