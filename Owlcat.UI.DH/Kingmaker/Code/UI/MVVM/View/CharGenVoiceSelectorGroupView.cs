using System.Linq;
using Kingmaker.UI.Common;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenVoiceSelectorGroupView : View<SelectionGroupRadioVM<CharGenVoiceItemVM>>, IConsoleEntityProxy, IConsoleEntity
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private CharGenVoiceItemView m_ItemPrefab;

	[SerializeField]
	private float m_EnsureVisibleFocusDelta = 100f;

	[SerializeField]
	private ScrollRectExtended m_ScrollRectExtended;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public IConsoleEntity ConsoleEntityProxy => m_NavigationBehaviour;

	protected override void OnBind()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusChanged).AddTo(this);
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
		UpdateNavigation();
	}

	private void UpdateNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddColumn(m_WidgetList.GetNavigationEntities());
	}

	public void SetFocus(bool value)
	{
		if (value)
		{
			m_NavigationBehaviour.FocusOnEntityManual(GetSelectedEntity());
		}
		else
		{
			m_NavigationBehaviour.UnFocusCurrentEntity();
		}
	}

	private IConsoleNavigationEntity GetSelectedEntity()
	{
		return m_WidgetList.Entries.Cast<CharGenVoiceItemView>().FirstOrDefault((CharGenVoiceItemView i) => i.GetViewModel()?.IsSelected.Value ?? false);
	}

	private void OnFocusChanged(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_ScrollRectExtended.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, m_EnsureVisibleFocusDelta, smoothly: false, needPinch: false);
		}
	}
}
