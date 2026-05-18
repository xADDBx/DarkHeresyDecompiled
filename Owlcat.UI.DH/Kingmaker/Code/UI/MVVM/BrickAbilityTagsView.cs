using System.Collections.Generic;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityTagsView : BrickBaseView<BrickAbilityTagsVM>
{
	private const string TagFormat = "| {0} |";

	[SerializeField]
	private AbilityTagWidget m_TagPrefab;

	[SerializeField]
	private RectTransform m_TagsParent;

	private List<AbilityTagWidget> m_PooledWidgets;

	protected override void OnBind()
	{
		if (m_PooledWidgets == null)
		{
			m_PooledWidgets = new List<AbilityTagWidget>();
		}
		foreach (string tag in base.ViewModel.Tags)
		{
			AbilityTagWidget widget = WidgetFactory.GetWidget(m_TagPrefab, activate: false);
			widget.SetText($"| {tag} |");
			widget.transform.SetParent(m_TagsParent, worldPositionStays: false);
			widget.gameObject.SetActive(value: true);
			m_PooledWidgets.Add(widget);
		}
	}

	protected override void OnUnbind()
	{
		foreach (AbilityTagWidget pooledWidget in m_PooledWidgets)
		{
			WidgetFactory.DisposeWidget(pooledWidget);
		}
		m_PooledWidgets.Clear();
	}
}
