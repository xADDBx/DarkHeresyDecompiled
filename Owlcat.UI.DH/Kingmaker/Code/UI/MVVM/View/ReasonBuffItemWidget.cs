using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class ReasonBuffItemWidget : View<ReasonBuffItemVM>
{
	[SerializeField]
	private Image m_IconImage;

	[SerializeField]
	private TMP_Text m_Text;

	protected override void OnBind()
	{
		m_IconImage.sprite = base.ViewModel.Icon;
		m_IconImage.gameObject.SetActive(base.ViewModel.Icon != null);
		m_Text.text = base.ViewModel.Name;
	}
}
