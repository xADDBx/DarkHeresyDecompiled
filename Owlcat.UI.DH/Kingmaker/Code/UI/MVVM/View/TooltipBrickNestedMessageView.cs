using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.Framework.GameLog;
using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class TooltipBrickNestedMessageView : TooltipBaseBrickView<TooltipBrickNestedMessageVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	protected Image m_Line;

	[SerializeField]
	private CanvasGroup m_PrefixCanvasGroup;

	[SerializeField]
	private Image m_IconImage;

	[SerializeField]
	private TextMeshProUGUI m_NumberText;

	[SerializeField]
	private RectTransform m_TooltipPlaceRectTransform;

	[SerializeField]
	protected CanvasGroup m_HighlightCanvasGroup;

	[SerializeField]
	private float m_DefaultFontSize = 17f;

	protected TooltipConfig m_TooltipConfig;

	public RectTransform TooltipPlace => m_TooltipPlaceRectTransform;

	protected override void OnBind()
	{
		m_Text.text = base.ViewModel.Text;
		SetTextFontSize();
		SetTextColor(base.ViewModel.TextColor);
		SetIcon();
		if (m_Line != null)
		{
			m_Line.gameObject.SetActive(base.ViewModel.NeedShowLine);
		}
		m_HighlightCanvasGroup.alpha = 0f;
		m_TooltipConfig.TooltipPlace = TooltipPlace;
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
	}

	private void SetIcon()
	{
		Sprite icon = GameLogUtility.GetIcon((base.ViewModel.ShotNumber > 0) ? PrefixIcon.Empty : base.ViewModel.PrefixIcon);
		if (icon != null)
		{
			m_IconImage.sprite = icon;
		}
		TextMeshProUGUI numberText = m_NumberText;
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

	private void SetTextFontSize()
	{
		m_Text.fontSize = m_DefaultFontSize * base.ViewModel.FontSizeMultiplier;
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
