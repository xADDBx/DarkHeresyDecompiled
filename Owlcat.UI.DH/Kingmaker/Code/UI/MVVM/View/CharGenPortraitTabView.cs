using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenPortraitTabView : SelectionGroupEntityView<CharGenPortraitTabVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Label.text = UtilityChargen.GetCharGenPortraitTabLabel(base.ViewModel.Tab);
	}

	public override void OnChangeSelectedState(bool value)
	{
		base.OnChangeSelectedState(value && base.ViewModel.IsMainCharacter.CurrentValue);
		m_Button.CanConfirm = base.ViewModel.IsMainCharacter.CurrentValue;
	}
}
