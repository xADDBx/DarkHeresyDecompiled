using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class TooltipBrickNestedMessageConsoleView : TooltipBrickNestedMessageView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IConsoleTooltipBrick
{
	[SerializeField]
	private OwlcatMultiButton m_FocusMultiButton;

	public bool IsBinded => base.ViewModel != null;

	protected override void OnBind()
	{
		base.OnBind();
		SetFocus(value: false);
	}

	public void SetFocus(bool value)
	{
		m_HighlightCanvasGroup.alpha = (value ? 1f : 0f);
	}

	public bool IsValid()
	{
		return true;
	}

	public bool CanConfirmClick()
	{
		return base.ViewModel.Unit != null;
	}

	public void OnConfirmClick()
	{
		OnConfirm();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public IConsoleEntity GetConsoleEntity()
	{
		return new SimpleConsoleNavigationEntity(m_FocusMultiButton, base.ViewModel.TooltipTemplate);
	}
}
