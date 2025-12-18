using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoClassesListPCView : View<CharInfoClassesListVM>
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private CharInfoClassEntryPCView m_ClassEntry;

	protected override void OnBind()
	{
		m_WidgetList.DrawEntries(base.ViewModel.ClassVMs, m_ClassEntry).AddTo(this);
	}
}
