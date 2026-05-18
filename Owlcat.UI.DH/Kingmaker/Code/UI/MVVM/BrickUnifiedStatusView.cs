using Code.View.UI.Helpers;
using Code.View.UI.MVVM;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickUnifiedStatusView : BrickBaseView<BrickUnifiedStatusVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Text;

	[SerializeField]
	private Image m_Image;

	[SerializeField]
	private Image m_Frame;

	[Header("Values")]
	[SerializeField]
	private EnumToObjectSelector<UnifiedStatus, UnifiedStatusParams> m_StatusParamsSelector = new EnumToObjectSelector<UnifiedStatus, UnifiedStatusParams>();

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Text).AddTo(this);
		}
		base.OnBind();
		m_Text.text = base.ViewModel.Text;
		ApplyStatus();
		m_TextHelper.UpdateTextSize();
	}

	private void ApplyStatus()
	{
		UnifiedStatusParams entity = m_StatusParamsSelector.GetEntity(base.ViewModel.Status);
		m_Text.color = entity.TextColor;
		m_Image.sprite = entity.Icon;
		m_Image.color = entity.IconColor;
		m_Frame.color = entity.FrameColor;
		m_Frame.gameObject.SetActive(entity.ShowFrame);
	}
}
