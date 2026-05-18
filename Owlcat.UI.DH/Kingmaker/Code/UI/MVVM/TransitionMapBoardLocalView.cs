using System.Linq;
using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM;

public class TransitionMapBoardLocalView : TransitionMapBoardBaseView
{
	[Header("Elements")]
	[SerializeField]
	private LocalMapPointPosition[] m_Positions = new LocalMapPointPosition[0];

	[SerializeField]
	private WidgetList m_MapList;

	[FormerlySerializedAs("MapEntityPrefab")]
	[Header("Views")]
	[SerializeField]
	private LocalTransitionMapEntityView TransitionMapEntityPrefab;

	[SerializeField]
	private LocalMapPaperListView m_PaperListView;

	private new TransitionMapLocalVM ViewModel => (TransitionMapLocalVM)base.ViewModel;

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_MapList.DrawEntries(ViewModel.ScreenEntities, TransitionMapEntityPrefab).AddTo(this);
		foreach (IBindable entry in m_MapList.Entries)
		{
			LocalTransitionMapEntityView view = entry as LocalTransitionMapEntityView;
			if ((object)view != null)
			{
				LocalMapPointPosition localMapPointPosition = m_Positions.FirstOrDefault((LocalMapPointPosition p) => p.IsDecent(view.ViewModel));
				if ((bool)localMapPointPosition)
				{
					view.transform.position = localMapPointPosition.GetPosition();
				}
			}
		}
		m_PaperListView.Bind(ViewModel.PaperListVM);
		EscHotkeyManager.Instance.Subscribe(delegate
		{
			ViewModel.Close();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}
}
