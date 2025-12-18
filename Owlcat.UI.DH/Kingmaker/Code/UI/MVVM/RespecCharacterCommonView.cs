using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class RespecCharacterCommonView : SelectionGroupEntityView<RespecCharacterVM>
{
	[SerializeField]
	private Image m_Image;

	[SerializeField]
	private TextMeshProUGUI m_CharacterName;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Image.sprite = base.ViewModel.UnitPortraitPartVM.Portrait.CurrentValue;
		m_CharacterName.text = base.ViewModel.CharacterName;
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		base.ViewModel.SetSelectedFromView(value);
	}
}
