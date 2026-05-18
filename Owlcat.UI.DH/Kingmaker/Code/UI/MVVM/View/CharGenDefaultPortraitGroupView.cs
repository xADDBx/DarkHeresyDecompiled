using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Common;
using ObservableCollections;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenDefaultPortraitGroupView : View<CharGenPortraitGroupVM>
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private CharGenPortraitSelectorItemView m_Prefab;

	[SerializeField]
	private ExpandableElement m_ExpandableElement;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private OwlcatMultiSelectable m_ExpandableSelectable;

	[SerializeField]
	private int m_PortraitsInRow = 7;

	protected override void OnBind()
	{
		m_ExpandableElement.Or(null)?.Initialize(delegate
		{
			base.ViewModel.SetExpanded(isExpanded: true);
		}, delegate
		{
			base.ViewModel.SetExpanded(isExpanded: false);
		});
		m_ExpandableElement.AddTo(this);
		if (m_Label != null)
		{
			m_Label.text = UtilityChargen.GetCharGenPortraitCategoryLabel(base.ViewModel.PortraitCategory);
		}
		base.ViewModel.PortraitCollection.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}).AddTo(this);
		base.ViewModel.Expanded.Subscribe(delegate(bool expanded)
		{
			if (expanded)
			{
				m_ExpandableElement.Or(null)?.Expand();
			}
			else
			{
				m_ExpandableElement.Or(null)?.Collapse();
			}
			m_ExpandableSelectable.SetActiveLayer(expanded ? "Expanded" : "Collapsed");
		}).AddTo(this);
		DrawEntities();
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.PortraitCollection, m_Prefab).AddTo(this);
	}
}
