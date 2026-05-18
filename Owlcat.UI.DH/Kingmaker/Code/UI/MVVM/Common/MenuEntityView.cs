using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.Common;

public class MenuEntityView : SelectionGroupEntityView<MenuEntityVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Title;

	protected override void OnBind()
	{
		base.OnBind();
		m_Title.text = base.ViewModel.Title.Text;
	}
}
