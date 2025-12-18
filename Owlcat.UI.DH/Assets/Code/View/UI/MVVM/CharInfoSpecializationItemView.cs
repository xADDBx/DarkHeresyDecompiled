using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code.View.UI.MVVM;

public class CharInfoSpecializationItemView : View<CharInfoSpecializationItemVM>
{
	[SerializeField]
	private TextMeshProUGUI m_SpecializationName;

	[SerializeField]
	private Image m_Icon;

	protected override void OnBind()
	{
		base.OnBind();
		m_SpecializationName.text = base.ViewModel.Name;
		m_Icon.sprite = base.ViewModel.Icon;
	}
}
