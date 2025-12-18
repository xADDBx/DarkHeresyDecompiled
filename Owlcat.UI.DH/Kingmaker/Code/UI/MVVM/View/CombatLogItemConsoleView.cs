using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CombatLogItemConsoleView : CombatLogItemBaseView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler
{
	[SerializeField]
	private CanvasGroup m_FocusCanvasGroup;

	[SerializeField]
	private CanvasGroup m_HighlightCanvasGroup;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 17f;

	public TooltipBaseTemplate TooltipTemplate => base.ViewModel.TooltipTemplate;

	protected override void OnBind()
	{
		SetFocus(value: false);
		base.OnBind();
		SetTextFontSize();
	}

	public void SetFocus(bool value)
	{
		m_FocusCanvasGroup.alpha = (value ? 1f : 0f);
		m_HighlightCanvasGroup.alpha = (value ? 1f : 0f);
	}

	public bool IsValid()
	{
		return true;
	}

	private void SetTextFontSize()
	{
		m_Text.fontSize = m_DefaultConsoleFontSize * base.ViewModel.FontSizeMultiplier;
	}

	public override void UpdateTextSize(float multiplier)
	{
		m_Text.fontSize = m_DefaultConsoleFontSize * multiplier;
		base.UpdateTextSize(multiplier);
	}

	public bool CanConfirmClick()
	{
		return base.ViewModel.Unit != null;
	}

	public void OnConfirmClick()
	{
		Game.Instance.Controllers.CameraController?.Follower?.ScrollTo(base.ViewModel.Unit);
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
