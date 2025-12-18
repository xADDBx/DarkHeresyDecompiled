using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarSlotWeaponAbilityConsoleView : ActionBarSlotWeaponAbilityView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IHasTooltipTemplate
{
	[Header("ConsoleSlot")]
	[SerializeField]
	private ActionBarSlotConsoleView m_SlotConsoleView;

	protected override void OnBind()
	{
		base.OnBind();
		m_SlotConsoleView.Bind(base.ViewModel);
	}

	public void SetFocus(bool value)
	{
		m_SlotConsoleView.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_SlotConsoleView.IsValid();
	}

	public bool CanConfirmClick()
	{
		return m_SlotConsoleView.CanConfirmClick();
	}

	public void OnConfirmClick()
	{
		m_SlotConsoleView.OnConfirmClick();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return m_SlotConsoleView.TooltipTemplate();
	}
}
