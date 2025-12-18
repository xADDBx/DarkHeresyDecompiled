using System.Collections.Generic;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class RespecCharactersSelectorView : View<SelectionGroupRadioVM<RespecCharacterVM>>
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private RespecCharacterCommonView m_RespecCharacterCommonView;

	protected override void OnBind()
	{
		DrawEntities();
	}

	protected override void OnUnbind()
	{
		m_WidgetList.Clear();
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_RespecCharacterCommonView);
	}

	public List<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return m_WidgetList.GetNavigationEntities();
	}
}
