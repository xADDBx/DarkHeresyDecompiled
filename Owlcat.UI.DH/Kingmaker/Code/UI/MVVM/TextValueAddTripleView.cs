using System;
using Kingmaker.Utility.Attributes;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TextValueAddTripleView : View<TextValueAddElement>
{
	[Header("Elements")]
	[SerializeField]
	protected TextEntityWidget m_Text;

	[SerializeField]
	private TextEntityWidget m_Value;

	[SerializeField]
	private TextEntityWidget m_AddValue;

	[ShowIf("m_HasTooltip")]
	[SerializeField]
	private MonoBehaviour m_TooltipRaycast;

	[Header("Values")]
	[SerializeField]
	private bool m_HasTooltip;

	private IDisposable m_TooltipDisposable;

	private IDisposable m_ValueHintDisposable;

	private void Awake()
	{
		m_Text.Initialize();
		m_Value.Initialize();
		m_AddValue.Initialize();
	}

	protected override void OnBind()
	{
		_ = m_HasTooltip;
		m_Text.Bind(base.ViewModel.Text)?.AddTo(this);
		m_Value.Bind(base.ViewModel.Value)?.AddTo(this);
		m_AddValue.Bind(base.ViewModel.AddValue)?.AddTo(this);
	}

	public void SetTooltip(TooltipBaseTemplate template, TooltipConfig config = default(TooltipConfig))
	{
		if (m_HasTooltip)
		{
			m_TooltipDisposable?.Dispose();
			m_TooltipDisposable = m_TooltipRaycast.SetTooltip(template).AddTo(this);
		}
	}

	public void SetValueHint(string hint)
	{
		m_ValueHintDisposable?.Dispose();
		if (!string.IsNullOrEmpty(hint))
		{
			m_Value.Text.SetHint(hint).AddTo(this);
		}
	}

	public void UpdateValue(string value)
	{
		m_Value.Text.text = value;
	}

	public void UpdateAddValue(string value)
	{
		m_AddValue.Text.text = value;
	}
}
