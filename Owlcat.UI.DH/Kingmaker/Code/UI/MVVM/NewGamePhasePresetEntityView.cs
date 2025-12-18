using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhasePresetEntityView : SelectionGroupEntityView<NewGamePhasePresetEntityVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	private Image m_Image;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Title.text = base.ViewModel.DisplayName;
		m_Description.text = base.ViewModel.Description;
		m_Image.sprite = base.ViewModel.Picture;
	}
}
