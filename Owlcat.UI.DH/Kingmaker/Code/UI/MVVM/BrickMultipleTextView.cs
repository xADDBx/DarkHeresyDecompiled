using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickMultipleTextView : BrickBaseView<BrickMultipleTextVM>
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private MultipleTextWidget m_Prefab;

	protected override void OnBind()
	{
		m_WidgetList.Clear();
		m_WidgetList.DrawEntries(base.ViewModel.Texts, m_Prefab);
	}
}
