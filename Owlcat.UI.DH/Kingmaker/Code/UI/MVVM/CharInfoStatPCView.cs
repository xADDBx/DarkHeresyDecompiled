using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoStatPCView : View<CharInfoStatVM>
{
	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_LongName;

	[SerializeField]
	private TextMeshProUGUI m_ShortName;

	[Header("Values")]
	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private bool m_AlwaysShowSign;

	[SerializeField]
	private float m_DefaultFontSize = 16f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 16f;

	protected override void OnBind()
	{
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
	}

	private void SetLabels()
	{
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		if (m_LongName != null)
		{
			m_LongName.text = base.ViewModel.Name.CurrentValue;
			m_LongName.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * base.ViewModel.FontMultiplier;
		}
		if (m_ShortName != null)
		{
			m_ShortName.text = base.ViewModel.ShortName;
			m_ShortName.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * base.ViewModel.FontMultiplier;
		}
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
