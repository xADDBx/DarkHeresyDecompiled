using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.Framework.GameLog;
using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickNestedMessageView : BrickBaseView<BrickNestedMessageVM>
{
	[SerializeField]
	private TMP_Text m_Text;

	[SerializeField]
	protected Image m_Line;

	[SerializeField]
	private CanvasGroup m_PrefixCanvasGroup;

	[SerializeField]
	private Image m_IconImage;

	[SerializeField]
	private TMP_Text m_NumberText;

	[SerializeField]
	private RectTransform m_TooltipPlaceRectTransform;

	[SerializeField]
	protected CanvasGroup m_HighlightCanvasGroup;

	private TooltipConfig m_TooltipConfig;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Text).AddTo(this);
		}
		m_Text.text = base.ViewModel.Text;
		SetTextColor(base.ViewModel.TextColor);
		SetIcon();
		m_HighlightCanvasGroup.alpha = 0f;
		m_TooltipConfig.TooltipPlace = m_TooltipPlaceRectTransform;
		if (Game.Instance.IsControllerMouse)
		{
			this.SetTooltip(base.ViewModel.TooltipTemplate, m_TooltipConfig).AddTo(this);
			this.OnPointerClickAsObservable().Subscribe(delegate(PointerEventData data)
			{
				OnClick(data);
			}).AddTo(this);
			this.OnPointerEnterAsObservable().Subscribe(delegate
			{
				m_HighlightCanvasGroup.alpha = 1f;
			}).AddTo(this);
			this.OnPointerExitAsObservable().Subscribe(delegate
			{
				m_HighlightCanvasGroup.alpha = 0f;
			}).AddTo(this);
		}
		m_TextHelper.UpdateTextSize();
	}

	private void SetIcon()
	{
		Sprite icon = GameLogUtility.GetIcon((base.ViewModel.ShotNumber > 0) ? PrefixIcon.Empty : base.ViewModel.PrefixIcon);
		if (icon != null)
		{
			m_IconImage.sprite = icon;
		}
		TMP_Text numberText = m_NumberText;
		int shotNumber = base.ViewModel.ShotNumber;
		numberText.text = shotNumber.ToString();
		m_NumberText.alpha = ((base.ViewModel.ShotNumber > 0) ? 1f : 0f);
		CanvasGroup prefixCanvasGroup = m_PrefixCanvasGroup;
		PrefixIcon prefixIcon = base.ViewModel.PrefixIcon;
		prefixCanvasGroup.alpha = ((prefixIcon == PrefixIcon.None || prefixIcon == PrefixIcon.Invisible) ? 0f : 1f);
	}

	private void SetTextColor(Color color)
	{
		m_Text.color = ((color.a > 0f) ? color : ((Color)GameLogStrings.Instance.DefaultColor));
	}

	private void OnClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			OnConfirm();
		}
	}

	protected void OnConfirm()
	{
		Game.Instance.Controllers.CameraController?.Follower?.ScrollTo(base.ViewModel.Unit);
	}
}
