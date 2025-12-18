using System.Collections.Generic;
using System.Linq;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class FormationSelectorPCView : View<SelectionGroupRadioVM<FormationSelectionItemVM>>
{
	[SerializeField]
	private FormationSelectionItemPCView m_Item0;

	[SerializeField]
	private FormationSelectionItemPCView m_Item1;

	[SerializeField]
	private FormationSelectionItemPCView m_Item2;

	[SerializeField]
	private FormationSelectionItemPCView m_Item3;

	[SerializeField]
	private FormationSelectionItemPCView m_Item4;

	[SerializeField]
	private FormationSelectionItemPCView m_Item5;

	private Dictionary<int, FormationSelectionItemPCView> m_ItemViews;

	public void Initialize()
	{
		m_ItemViews = new Dictionary<int, FormationSelectionItemPCView>
		{
			{ 0, m_Item0 },
			{ 1, m_Item1 },
			{ 2, m_Item2 },
			{ 3, m_Item3 },
			{ 4, m_Item4 },
			{ 5, m_Item5 }
		};
	}

	protected override void OnBind()
	{
		foreach (KeyValuePair<int, FormationSelectionItemPCView> itemView in m_ItemViews)
		{
			var (key, formationSelectionItemPCView2) = (KeyValuePair<int, FormationSelectionItemPCView>)(ref itemView);
			formationSelectionItemPCView2.Bind(base.ViewModel.EntitiesCollection.FirstOrDefault((FormationSelectionItemVM s) => s.FormationIndex == key));
		}
	}
}
