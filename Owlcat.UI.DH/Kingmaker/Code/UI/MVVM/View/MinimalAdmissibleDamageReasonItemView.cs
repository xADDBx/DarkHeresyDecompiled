using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class MinimalAdmissibleDamageReasonItemView : View<MinimalAdmissibleDamageReasonItemVM>
{
	[SerializeField]
	private Image m_IconImage;

	[SerializeField]
	private TextMeshProUGUI m_Text;

	protected override void OnBind()
	{
		m_IconImage.color = Color.black;
		m_Text.text = base.ViewModel.Text;
	}
}
