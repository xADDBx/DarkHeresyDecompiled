using System.Linq;
using ObservableCollections;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class TextureSelectorGroupView : View<SelectionGroupRadioVM<TextureSelectorItemVM>>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private GameObject m_DescriptionObject;

	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	protected WidgetList m_WidgetList;

	[SerializeField]
	private TextureSelectorItemView m_ItemPrefab;

	[SerializeField]
	protected int m_ItemsPerRow;

	public bool IsActive => (base.ViewModel?.EntitiesCollection?.Any()).GetValueOrDefault();

	protected override void OnBind()
	{
		DrawEntities();
		base.ViewModel.EntitiesCollection.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}).AddTo(this);
	}

	public void SetTitleText(string title)
	{
		if (!(m_Label == null))
		{
			m_Label.gameObject.SetActive(!string.IsNullOrEmpty(title));
			m_Label.text = title;
		}
	}

	public void SetDescriptionText(string description)
	{
		if (!(m_Description == null))
		{
			m_DescriptionObject.SetActive(!string.IsNullOrEmpty(description));
			m_Description.text = description;
		}
	}

	protected virtual void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection.ToArray(), m_ItemPrefab).AddTo(this);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}
}
