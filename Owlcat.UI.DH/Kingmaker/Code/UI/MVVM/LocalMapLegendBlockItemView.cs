using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapLegendBlockItemView : View<LocalMapLegendBlockItemVM>
{
	[SerializeField]
	private Image m_ItemImage;

	[SerializeField]
	private TextMeshProUGUI m_ItemLabel;

	protected override void OnBind()
	{
		m_ItemImage.sprite = base.ViewModel.ItemSprite;
		m_ItemLabel.text = base.ViewModel.ItemLabel;
	}
}
