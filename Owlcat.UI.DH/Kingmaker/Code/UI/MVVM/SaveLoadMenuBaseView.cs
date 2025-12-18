using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SaveLoadMenuBaseView : View<SaveLoadMenuVM>
{
	[SerializeField]
	private SaveLoadMenuSelectorBaseView m_Selector;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_Selector.Initialize();
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		m_Selector.Bind(base.ViewModel.SelectionGroup);
	}
}
