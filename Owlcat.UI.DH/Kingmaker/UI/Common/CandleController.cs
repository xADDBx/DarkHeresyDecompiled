using System;
using Kingmaker.Code.UI.MVVM;
using Owlcat.Runtime.Core.Utility;
using R3;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace Kingmaker.UI.Common;

public class CandleController : MonoBehaviour
{
	[SerializeField]
	private GameObject m_CandleFx;

	[SerializeField]
	private ImageFlicker m_ImageFlicker;

	private IDisposable m_Subscription;

	private TooltipContext TooltipContext => RootVM.Instance?.TooltipContext;

	private void OnEnable()
	{
		if (TooltipContext == null)
		{
			return;
		}
		m_Subscription = TooltipContext.TooltipVM.CombineLatest(TooltipContext.HintVM, TooltipContext.InfoWindowVM, TooltipContext.GlossaryInfoWindowVM, TooltipContext.ComparativeTooltipVM, (TooltipVM tooltip, HintVM hint, InfoWindowVM info, InfoWindowVM glossary, ComparativeTooltipVM comparative) => new { tooltip, hint, info, glossary, comparative }).Subscribe(value =>
		{
			bool flag = value.tooltip != null || value.hint != null || value.info != null || value.glossary != null || value.comparative != null;
			m_CandleFx.Or(null)?.SetActive(!flag);
			if (m_ImageFlicker != null)
			{
				m_ImageFlicker.enabled = !flag;
			}
		});
	}

	private void OnDisable()
	{
		m_Subscription?.Dispose();
		m_Subscription = null;
	}
}
