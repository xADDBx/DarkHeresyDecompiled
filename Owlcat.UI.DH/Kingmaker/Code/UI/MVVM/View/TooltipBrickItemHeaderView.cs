using Code.View.UI.Helpers;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class TooltipBrickItemHeaderView : TooltipBaseBrickView<TooltipBrickItemHeaderVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_Text;

	[Tooltip("Has one of states of ItemHeaderType enum: Default, Header, Equipped, CanNotEquip")]
	[SerializeField]
	private OwlcatMultiSelectable m_Selectable;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Text);
		}
		base.OnBind();
		m_Text.text = base.ViewModel.Text;
		m_Selectable.SetActiveLayer(base.ViewModel.Type.ToString());
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_TextHelper.Dispose();
	}
}
