using System.Linq;
using Kingmaker.UI.Common;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenVoiceSelectorGroupView : View<SelectionGroupRadioVM<CharGenVoiceItemVM>>
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private CharGenVoiceItemView m_ItemPrefab;

	[SerializeField]
	private float m_EnsureVisibleFocusDelta = 100f;

	[SerializeField]
	private ScrollRectExtended m_ScrollRectExtended;

	protected override void OnBind()
	{
		DrawEntities();
		base.ViewModel.EntitiesCollection.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}).AddTo(this);
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection.ToArray(), m_ItemPrefab).AddTo(this);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}
}
