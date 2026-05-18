using Code.View.UI.Helpers;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class BrickTriggeredAutoView : BrickBaseView<BrickTriggeredAutoVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_TriggeredAutoText;

	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private ReasonBuffItemWidget ReasonBuffItemWidget;

	[Header("Selectables")]
	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	protected override void OnBind()
	{
		m_TextHelper = new AccessibilityTextHelper(m_TriggeredAutoText);
		m_TriggeredAutoText.text = base.ViewModel.TriggeredAutoText;
		if (base.ViewModel.ReasonBuffItems.AnyItem())
		{
			m_WidgetList.DrawEntries(base.ViewModel.ReasonBuffItems, ReasonBuffItemWidget).AddTo(this);
		}
		m_StateSelectable.SetActiveLayer(base.ViewModel.IsSuccess ? "Success" : "Failed");
		m_TextHelper.UpdateTextSize();
	}
}
