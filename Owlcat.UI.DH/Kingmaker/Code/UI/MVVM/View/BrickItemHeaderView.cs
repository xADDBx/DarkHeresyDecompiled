using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class BrickItemHeaderView : BrickBaseView<BrickItemHeaderVM>
{
	[SerializeField]
	protected TMP_Text m_Text;

	[Tooltip("Has one of states of ItemHeaderType enum: Default, Header, Equipped, CanNotEquip")]
	[SerializeField]
	private OwlcatMultiSelectable m_Selectable;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Text).AddTo(this);
		}
		base.OnBind();
		m_Text.text = base.ViewModel.Text;
		m_Selectable.SetActiveLayer(base.ViewModel.Type.ToString());
		m_TextHelper.UpdateTextSize();
	}
}
