using Code.View.UI.Helpers;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class TooltipBrickTriggeredAutoView : TooltipBaseBrickView<TooltipBrickTriggeredAutoVM>
{
	[SerializeField]
	private TextMeshProUGUI m_TriggeredAutoText;

	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private ReasonBuffItemView m_ReasonBuffItemView;

	[SerializeField]
	private Image m_ResultSignImage;

	[Header("Sprites")]
	[SerializeField]
	private Sprite m_ResultSignSuccessSprite;

	[SerializeField]
	private Sprite m_ResultSignFailedSprite;

	[Header("Colors")]
	[SerializeField]
	private Color m_OrangeColor;

	[SerializeField]
	private Color m_LightColor;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		m_TextHelper = new AccessibilityTextHelper(m_TriggeredAutoText);
		m_TriggeredAutoText.text = base.ViewModel.TriggeredAutoText;
		if (base.ViewModel.ReasonBuffItems.AnyItem())
		{
			m_WidgetList.DrawEntries(base.ViewModel.ReasonBuffItems, m_ReasonBuffItemView).AddTo(this);
		}
		m_ResultSignImage.sprite = (base.ViewModel.IsSuccess ? m_ResultSignSuccessSprite : m_ResultSignFailedSprite);
		m_ResultSignImage.color = (base.ViewModel.IsSuccess ? m_OrangeColor : m_LightColor);
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_TextHelper.Dispose();
	}
}
