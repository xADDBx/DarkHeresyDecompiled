using System;
using Kingmaker.Utility.Attributes;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TextValueTupleView : View<TextValueElement>
{
	[Header("Elements")]
	[SerializeField]
	private TextEntityWidget m_Text;

	[SerializeField]
	private TextEntityWidget m_Value;

	[ShowIf("m_HasTooltip")]
	[SerializeField]
	private MonoBehaviour m_TooltipRaycast;

	[Header("Values")]
	[SerializeField]
	private bool m_HasTooltip;

	private IDisposable m_TooltipDisposable;

	public TMP_Text Text => m_Text.Text;

	public TMP_Text Value => m_Value.Text;

	private void Awake()
	{
		m_Text.Initialize();
		m_Value.Initialize();
	}

	protected override void OnBind()
	{
		_ = m_HasTooltip;
		m_Text.Bind(base.ViewModel.Text)?.AddTo(this);
		m_Value.Bind(base.ViewModel.Value)?.AddTo(this);
	}

	public void SetTooltip(TooltipBaseTemplate template, TooltipConfig config = default(TooltipConfig))
	{
		if (m_HasTooltip)
		{
			m_TooltipDisposable?.Dispose();
			m_TooltipDisposable = m_TooltipRaycast.SetTooltip(template).AddTo(this);
		}
	}
}
