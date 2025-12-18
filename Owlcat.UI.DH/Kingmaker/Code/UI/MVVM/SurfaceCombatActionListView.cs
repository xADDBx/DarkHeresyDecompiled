using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SurfaceCombatActionListView : View<SurfaceCombatMechanicEntityActionListVM>
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private SurfaceCombatActionView m_SurfaceCombatActionView;

	public void Awake()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		base.ViewModel.Actions.ObserveCountChanged().Subscribe(delegate
		{
			DrawActions();
		}).AddTo(this);
		DrawActions();
	}

	private void DrawActions()
	{
		m_WidgetList.DrawEntries(base.ViewModel.Actions, m_SurfaceCombatActionView);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
		m_WidgetList.Clear();
	}
}
