using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenPortraitTabView : SelectionGroupEntityView<CharGenPortraitTabVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	protected override void OnBind()
	{
		base.OnBind();
		m_Label.text = UtilityChargen.GetCharGenPortraitTabLabel(base.ViewModel.Tab);
		m_Button.Interactable = base.ViewModel.Tab != CharGenPortraitTab.Custom;
	}

	public override void OnChangeSelectedState(bool value)
	{
		base.OnChangeSelectedState(value && base.ViewModel.IsMainCharacter.CurrentValue);
		m_Button.CanConfirm = base.ViewModel.IsMainCharacter.CurrentValue;
	}
}
