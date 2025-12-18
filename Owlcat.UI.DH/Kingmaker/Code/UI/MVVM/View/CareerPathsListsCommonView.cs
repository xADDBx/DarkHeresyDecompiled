using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Core.Utility;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CareerPathsListsCommonView : CharInfoComponentView<UnitProgressionVM>
{
	[SerializeField]
	private RankExpCounterCommonView m_RankExpCounterCommonView;

	[SerializeField]
	private UnitBackgroundBlockCommonView m_UnitBackgroundBlockCommonView;

	[SerializeField]
	protected List<CareerPathsListCommonView> m_CareerPathsLists;

	protected readonly ReactiveProperty<bool> m_IsShown = new ReactiveProperty<bool>();

	public override void Initialize()
	{
		base.Initialize();
		foreach (CareerPathsListCommonView careerPathsList in m_CareerPathsLists)
		{
			careerPathsList.Initialize();
		}
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_RankExpCounterCommonView.Or(null)?.Bind(base.ViewModel.CharInfoExperienceVM);
		m_UnitBackgroundBlockCommonView.Or(null)?.Bind(base.ViewModel.UnitBackgroundBlockVM);
		CreateCharGenLines();
	}

	protected override void RefreshView()
	{
		for (int i = 0; i < m_CareerPathsLists.Count; i++)
		{
			CareerPathsListVM careerPathsListVM = base.ViewModel.CareerPathsList.ElementAtOrDefault(i);
			m_CareerPathsLists[i].Bind(careerPathsListVM);
			m_CareerPathsLists[i].gameObject.SetActive(careerPathsListVM != null);
		}
	}

	public void SetVisibility(bool visible)
	{
		m_FadeAnimator.PlayAnimation(visible);
		m_IsShown.Value = visible;
	}

	[ContextMenu("TestMe")]
	public void CreateCharGenLines()
	{
		if (!base.ViewModel.IsCharGen)
		{
			return;
		}
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			List<CareerPathListItemCommonView> allCareerViews = m_CareerPathsLists.SelectMany((CareerPathsListCommonView l) => l.ItemViews).ToList();
			m_CareerPathsLists.ForEach(delegate(CareerPathsListCommonView l)
			{
				l.CreateLines(allCareerViews);
			});
		}).AddTo(this);
	}
}
