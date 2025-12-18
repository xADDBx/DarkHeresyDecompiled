using System;
using System.Linq;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class UnitProgressionCommonView : CharInfoComponentView<UnitProgressionVM>
{
	[Header("Breadcrumbs")]
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private GameObject m_BreadcrumbsSlashes;

	[SerializeField]
	private ProgressionBreadcrumbsItemCommonView m_ProgressionBreadcrumbsItemCommonView;

	[Header("Sub Views")]
	[SerializeField]
	protected CareerPathsListsCommonView m_CareerPathsListsCommonView;

	[SerializeField]
	protected CareerPathProgressionCommonView m_CareerPathProgressionCommonView;

	private GameObject m_SpawnedSlashed;

	private UnitProgressionWindowState m_CurrentState;

	private Action<UnitProgressionWindowState> m_OnWindowStateChange;

	public UnitProgressionWindowState CurrentState => m_CurrentState;

	public void Initialize(Action<UnitProgressionWindowState> onWindowStateChange)
	{
		base.Initialize();
		m_OnWindowStateChange = onWindowStateChange;
		m_CareerPathsListsCommonView.Initialize();
		m_CareerPathProgressionCommonView.Initialize(HandleReturnAction);
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_CareerPathsListsCommonView.Bind(base.ViewModel);
		base.ViewModel.CurrentCareer.Subscribe(BindPathProgression).AddTo(this);
		base.ViewModel.State.Subscribe(HandleState).AddTo(this);
		base.ViewModel.Breadcrumbs.ObserveCountChanged().Subscribe(delegate
		{
			DrawBreadcrumbs();
		}).AddTo(this);
		DrawBreadcrumbs();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_CareerPathsListsCommonView.Unbind();
		m_CareerPathProgressionCommonView.Unbind();
		m_WidgetList.Clear();
	}

	private void DrawBreadcrumbs()
	{
		if ((bool)m_SpawnedSlashed)
		{
			UnityEngine.Object.Destroy(m_SpawnedSlashed);
		}
		m_WidgetList.DrawEntries(base.ViewModel.Breadcrumbs.ToArray(), m_ProgressionBreadcrumbsItemCommonView);
		m_SpawnedSlashed = UnityEngine.Object.Instantiate(m_BreadcrumbsSlashes, m_WidgetList.transform, worldPositionStays: false);
		m_SpawnedSlashed.transform.SetAsFirstSibling();
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_WidgetList.transform as RectTransform);
	}

	private void HandleReturnAction(bool saveSelections = false)
	{
		base.ViewModel.SetPreviousState(saveSelections);
	}

	protected virtual void HandleState(UnitProgressionWindowState state)
	{
		m_CurrentState = state;
		m_OnWindowStateChange?.Invoke(state);
		m_CareerPathsListsCommonView.SetVisibility(state == UnitProgressionWindowState.CareerPathList);
		m_CareerPathProgressionCommonView.SetVisibility(state == UnitProgressionWindowState.CareerPathProgression);
	}

	protected virtual void BindPathProgression(CareerPathVM careerPathVM)
	{
		m_CareerPathProgressionCommonView.Bind(careerPathVM);
		m_CareerPathsListsCommonView.Bind((careerPathVM == null) ? base.ViewModel : null);
	}
}
