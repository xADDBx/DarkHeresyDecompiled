using System;
using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.UI;

[Obsolete]
public class SimpleConsoleNavigationEntity : IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate, IMonoBehaviour, IConfirmClickHandler
{
	private readonly MonoBehaviour m_ButtonAsMonoBehaviour;

	private readonly IConsoleNavigationEntity m_Button;

	private readonly TooltipBaseTemplate m_Tooltip;

	private readonly Action m_OnConfirm;

	private readonly MonoBehaviour m_ManualTooltipPlace;

	public MonoBehaviour MonoBehaviour => (MonoBehaviour)m_Button;

	public SimpleConsoleNavigationEntity(IConsoleNavigationEntity button, TooltipBaseTemplate tooltip = null, Action onConfirm = null, MonoBehaviour tooltipPlace = null)
	{
		m_Button = button;
		m_ButtonAsMonoBehaviour = button as MonoBehaviour;
		m_Tooltip = tooltip;
		m_OnConfirm = onConfirm;
		m_ManualTooltipPlace = tooltipPlace;
	}

	public void SetFocus(bool value)
	{
		if (m_Button != null)
		{
			m_Button.SetFocus(value);
		}
	}

	public bool IsValid()
	{
		if (m_Button != null)
		{
			return m_Button.IsValid();
		}
		return false;
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return m_Tooltip;
	}

	public bool CanConfirmClick()
	{
		return m_OnConfirm != null;
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public MonoBehaviour GetTooltipPlace()
	{
		return m_ManualTooltipPlace;
	}

	public void OnConfirmClick()
	{
		m_OnConfirm?.Invoke();
	}

	public Vector2 GetPosition()
	{
		return m_ButtonAsMonoBehaviour.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}
}
