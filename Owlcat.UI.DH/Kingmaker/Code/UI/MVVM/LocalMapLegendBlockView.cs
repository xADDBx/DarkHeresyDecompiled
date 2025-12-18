using System.Collections.Generic;
using DG.Tweening;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapLegendBlockView : View<LocalMapLegendBlockVM>
{
	[SerializeField]
	private CanvasGroup m_BlockCanvasGroup;

	[SerializeField]
	private LocalMapLegendBlockItemView m_LocalMapLegendBlockItemViewPrefab;

	[SerializeField]
	private List<RectTransform> m_ItemsPanels;

	[SerializeField]
	private int m_ItemsInOnePanel;

	private readonly List<LocalMapLegendBlockItemView> m_LegendItems = new List<LocalMapLegendBlockItemView>();

	protected override void OnBind()
	{
		m_BlockCanvasGroup.alpha = 0f;
		AddItems();
	}

	protected override void OnUnbind()
	{
		m_LegendItems.ForEach(WidgetFactory.DisposeWidget);
		m_ItemsPanels.ForEach(delegate(RectTransform b)
		{
			b.gameObject.SetActive(value: false);
		});
	}

	private void AddItems()
	{
		int num = 0;
		int num2 = 0;
		m_ItemsPanels[num].gameObject.SetActive(value: true);
		foreach (LocalMapLegendBlockItemVM localMapItemsVM in base.ViewModel.LocalMapItemsVMs)
		{
			if (m_ItemsPanels.Count < num)
			{
				break;
			}
			LocalMapLegendBlockItemView widget = WidgetFactory.GetWidget(m_LocalMapLegendBlockItemViewPrefab, activate: true, strictMatching: true);
			widget.Bind(localMapItemsVM);
			widget.transform.SetParent(m_ItemsPanels[num], worldPositionStays: false);
			num2++;
			m_LegendItems.Add(widget);
			if (num2 == m_ItemsInOnePanel)
			{
				num2 = 0;
				num++;
				m_ItemsPanels[num].gameObject.SetActive(value: true);
			}
		}
	}

	public void ShowHide(bool state)
	{
		m_BlockCanvasGroup.DOFade(state ? 1 : 0, 0.1f).SetEase(Ease.Linear).SetUpdate(isIndependentUpdate: true);
	}
}
