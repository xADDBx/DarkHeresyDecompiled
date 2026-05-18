using Code.View.UI.Helpers;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class BrickTextValueView : BrickBaseView<BrickTextValueVM>
{
	[SerializeField]
	private TMP_Text m_Text;

	[SerializeField]
	private TMP_Text m_Value;

	[SerializeField]
	protected GameObject m_ResultLineImage;

	[SerializeField]
	protected Image m_Line;

	[Header("Selectables")]
	[SerializeField]
	private OwlcatMultiSelectable m_NestedSelectable;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Text, m_Value);
		}
		m_Text.text = base.ViewModel.Text;
		m_Value.text = base.ViewModel.Value;
		m_NestedSelectable.SetActiveLayer(base.ViewModel.NestedLevel);
		m_ResultLineImage.SetActive(base.ViewModel.IsResultValue);
		m_Line.gameObject.SetActive(base.ViewModel.NeedShowLine);
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_TextHelper.Dispose();
	}
}
