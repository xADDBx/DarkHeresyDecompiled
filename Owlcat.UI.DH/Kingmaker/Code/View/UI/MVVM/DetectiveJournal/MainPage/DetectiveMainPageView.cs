using System.Collections.Generic;
using Kingmaker.UI.Common;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal.MainPage;

public class DetectiveMainPageView : View<DetectiveMainPageVM>
{
	[Header("Elements")]
	[SerializeField]
	private WidgetList m_CasesContainer;

	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	[SerializeField]
	private OwlcatMultiButton m_ShowClosedCasesToggle;

	[SerializeField]
	private GameObject m_EmptyCasesContainer;

	[Header("Views")]
	[SerializeField]
	private MainPageCardBaseView m_CardPrefab;

	[SerializeField]
	private GameObject m_EmptyFolder;

	[SerializeField]
	private DetectiveMainPageDecorView m_DecorView;

	[Header("Values")]
	[SerializeField]
	private int m_MinItemsCount = 8;

	private readonly List<GameObject> m_EmptyViews = new List<GameObject>();

	protected override void OnBind()
	{
		m_DecorView.Bind(default(Unit));
		base.ViewModel.HasAnyFolder.Subscribe(delegate(bool value)
		{
			m_EmptyCasesContainer.SetActive(!value);
			m_ShowClosedCasesToggle.gameObject.SetActive(value);
			m_ScrollRect.gameObject.SetActive(value);
		}).AddTo(this);
		base.ViewModel.ShowClosedCases.Subscribe(delegate(bool value)
		{
			m_ShowClosedCasesToggle.SetActiveLayer(value ? 1 : 0);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ShowClosedCasesToggle.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ToggleClosedCases();
		}).AddTo(this);
		base.ViewModel.Cases.ObserveAdd().Subscribe(delegate
		{
			DrawEntries();
		}).AddTo(this);
		base.ViewModel.Cases.ObserveRemove().Subscribe(delegate
		{
			DrawEntries();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.Cases.ObserveReset(), delegate
		{
			DrawEntries();
		}).AddTo(this);
		DrawEntries();
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			m_ScrollRect.ScrollToTop();
		}).AddTo(this);
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
		CleatEmptySlots();
	}

	private void DrawEntries()
	{
		CleatEmptySlots();
		m_CasesContainer.Clear();
		m_CasesContainer.DrawEntries(base.ViewModel.Cases, m_CardPrefab).AddTo(this);
		for (int i = m_CasesContainer.Entries.Count; i < m_MinItemsCount; i++)
		{
			GameObject gameObject = Object.Instantiate(m_EmptyFolder, m_CasesContainer.transform, worldPositionStays: false);
			m_EmptyViews.Add(gameObject);
			gameObject.transform.SetAsLastSibling();
		}
	}

	private void CleatEmptySlots()
	{
		m_EmptyViews.ForEach(Object.Destroy);
		m_EmptyViews.Clear();
	}
}
