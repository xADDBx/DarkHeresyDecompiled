using Kingmaker.Utility.Attributes;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class DialogNotificationView : View<DialogNotificationVM>
{
	[Header("Elements")]
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private bool m_HasIcon = true;

	[SerializeField]
	[ShowIf("m_HasIcon")]
	private Image m_Icon;

	[SerializeField]
	private LayoutElement m_LayoutElement;

	[Header("Values")]
	[SerializeField]
	private TooltipConfig m_TooltipConfig;

	protected override void OnBind()
	{
		m_Text.text = base.ViewModel.Label;
		m_Text.SetLinkTooltip(null, null, m_TooltipConfig).AddTo(this);
		if (m_HasIcon)
		{
			m_LayoutElement.minHeight = ((base.ViewModel.Icon != null) ? 62f : (-1f));
			m_Icon.gameObject.SetActive(base.ViewModel.Icon != null);
			m_Icon.sprite = base.ViewModel.Icon;
			m_Icon.SetTooltip(base.ViewModel.IconTooltip, m_TooltipConfig).AddTo(this);
		}
		base.OnBind();
	}

	protected override void OnUnbind()
	{
		if (m_HasIcon)
		{
			m_Text.text = string.Empty;
			m_Icon.gameObject.SetActive(value: false);
		}
		base.OnUnbind();
	}

	private void OnDisable()
	{
		TooltipHelper.HideTooltip();
	}
}
