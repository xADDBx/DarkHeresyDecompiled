using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class AdditionalCombatHintView : View<AdditionalCombatHintVM>
{
	[Header("Elements")]
	[SerializeField]
	private WidgetList m_WidgetList;

	[Header("Views")]
	[SerializeField]
	private CombatHintEntityView m_CombatHintEntityView;

	protected override void OnBind()
	{
		m_WidgetList.DrawEntries(base.ViewModel.CombatObjectives, m_CombatHintEntityView);
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
		m_WidgetList.Clear();
	}
}
