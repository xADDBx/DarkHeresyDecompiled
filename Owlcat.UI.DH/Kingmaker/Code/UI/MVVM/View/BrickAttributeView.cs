using Code.View.UI.Helpers;
using Code.View.UI.MVVM;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class BrickAttributeView : BrickBaseView<BrickAttributeVM>
{
	[Header("Elements")]
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private GameObject m_RecommendedMark;

	[Header("Widgets")]
	[SerializeField]
	private EnumToObjectSelector<StripeType, TextWithParent> m_WidgetByTypeSelector;

	private AccessibilityTextHelper m_AccessibilityTextHelper;

	protected override void OnBind()
	{
		m_Label.text = base.ViewModel.Name;
		m_WidgetByTypeSelector.EntitiesWithTypes.ForEach(delegate(EnumToObjectSelector<StripeType, TextWithParent>.Entity e)
		{
			e?.Value.Container.SetActive(value: false);
		});
		TextWithParent entity = m_WidgetByTypeSelector.GetEntity(base.ViewModel.StripeType);
		entity.Text.SetText(base.ViewModel.Acronym);
		entity.Container.SetActive(value: true);
		m_RecommendedMark.SetActive(base.ViewModel.IsRecommended);
		this.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		if (m_AccessibilityTextHelper == null)
		{
			m_AccessibilityTextHelper = new AccessibilityTextHelper(m_Label);
		}
		m_AccessibilityTextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_AccessibilityTextHelper.Dispose();
		m_AccessibilityTextHelper = null;
	}
}
