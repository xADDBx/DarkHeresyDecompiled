using System.Linq;
using ObservableCollections;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class TextureSelectorGroupView : View<SelectionGroupRadioVM<TextureSelectorItemVM>>, IConsoleNavigationEntity, IConsoleEntity, INavigationDirectionsHandler, INavigationVerticalDirectionsHandler, INavigationUpDirectionHandler, INavigationDownDirectionHandler, INavigationHorizontalDirectionsHandler, INavigationLeftDirectionHandler, INavigationRightDirectionHandler, IConfirmClickHandler
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

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public bool IsActive => (base.ViewModel?.EntitiesCollection?.Any()).GetValueOrDefault();

	public virtual bool CanConfirmClick()
	{
		return m_NavigationBehaviour.CanConfirmClick();
	}

	public virtual void OnConfirmClick()
	{
		m_NavigationBehaviour.OnConfirmClick();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public virtual void SetFocus(bool value)
	{
		if (value)
		{
			m_NavigationBehaviour.FocusOnEntityManual(GetSelectedEntity());
			return;
		}
		GetSelectedItemVM()?.SetSelected(state: true);
		m_NavigationBehaviour.UnFocusCurrentEntity();
	}

	public bool IsValid()
	{
		return true;
	}

	public virtual bool HandleUp()
	{
		return m_NavigationBehaviour.HandleUp();
	}

	public virtual bool HandleDown()
	{
		return m_NavigationBehaviour.HandleDown();
	}

	public virtual bool HandleLeft()
	{
		return m_NavigationBehaviour.HandleLeft();
	}

	public virtual bool HandleRight()
	{
		return m_NavigationBehaviour.HandleRight();
	}

	protected override void OnBind()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
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
		UpdateNavigation();
	}

	protected virtual void UpdateNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.SetEntitiesGrid(m_WidgetList.GetNavigationEntities(), m_ItemsPerRow);
	}

	private TextureSelectorItemVM GetSelectedItemVM()
	{
		return (from TextureSelectorItemView view in m_WidgetList.Entries
			select view.GetViewModel()).FirstOrDefault((TextureSelectorItemVM vm) => vm?.IsSelected.Value ?? false);
	}

	private IConsoleNavigationEntity GetSelectedEntity()
	{
		return m_WidgetList.Entries.Cast<TextureSelectorItemView>().FirstOrDefault((TextureSelectorItemView i) => i.GetViewModel()?.IsSelected.Value ?? false);
	}
}
