using System.Collections.Generic;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenCustomPortraitGroupView : View<CharGenPortraitGroupVM>
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private CharGenPortraitSelectorItemView m_Prefab;

	[SerializeField]
	private CharGenCustomPortraitCreatorItemView m_CreatorItemPrefab;

	[SerializeField]
	private int m_ItemsInRow;

	protected override void OnBind()
	{
		base.ViewModel.PortraitCollection.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}).AddTo(this);
		DrawEntities();
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawMultiEntries(base.ViewModel.PortraitCollection, new List<MonoBehaviour> { m_Prefab, m_CreatorItemPrefab }).AddTo(this);
	}
}
