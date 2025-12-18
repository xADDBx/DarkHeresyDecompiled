using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View;

public class CombatLogItemPCView : CombatLogItemBaseView, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	[SerializeField]
	private CanvasGroup m_HighlightCanvasGroup;

	[SerializeField]
	private float m_DefaultFontSize = 17f;

	[SerializeField]
	private TooltipConfig m_TooltipConfig;

	protected override void OnBind()
	{
		base.OnBind();
		this.SetTooltip(base.ViewModel.TooltipTemplate, m_TooltipConfig).AddTo(this);
		m_HighlightCanvasGroup.alpha = 0f;
		SetTextFontSize();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		m_HighlightCanvasGroup.alpha = 1f;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		m_HighlightCanvasGroup.alpha = 0f;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			Game.Instance.Controllers.CameraController?.Follower?.ScrollTo(base.ViewModel.Unit);
		}
	}

	private void SetTextFontSize()
	{
		m_Text.fontSize = m_DefaultFontSize * base.ViewModel.FontSizeMultiplier;
	}

	public override void UpdateTextSize(float multiplier)
	{
		m_Text.fontSize = m_DefaultFontSize * multiplier;
		base.UpdateTextSize(multiplier);
	}
}
