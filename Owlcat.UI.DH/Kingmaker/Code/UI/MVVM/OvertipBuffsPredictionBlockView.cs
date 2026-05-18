using System.Collections.Generic;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipBuffsPredictionBlockView : View<OvertipBuffsPredictionBlockVM>
{
	[SerializeField]
	private GridLayoutGroup m_BuffsGrid;

	[SerializeField]
	private BuffPredictionWidget m_BuffPrefab;

	[SerializeField]
	private int m_OneColumnLimit = 3;

	[SerializeField]
	private int m_MultipleColumnsLimit = 2;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	private List<BuffPredictionWidget> m_PooledWidgets = new List<BuffPredictionWidget>();

	public void HideInstant()
	{
		m_FadeAnimator.DisappearInstant();
	}

	protected override void OnBind()
	{
		base.ViewModel.Buffs.Subscribe(HandleBuffsChanged).AddTo(this);
		base.ViewModel.IsVisible.Subscribe(HandleVisibilityChanged).AddTo(this);
	}

	protected override void OnUnbind()
	{
		ClearWidgets();
	}

	private void HandleVisibilityChanged(bool isVisible)
	{
		m_FadeAnimator.PlayAnimation(isVisible);
	}

	private void HandleBuffsChanged(IReadOnlyList<OvertipBuffsPredictionBlockVM.BuffPrediction> buffs)
	{
		if (buffs.Count < 1)
		{
			m_FadeAnimator.PlayAnimation(value: false);
			return;
		}
		SetupWidgets(buffs);
		m_FadeAnimator.PlayAnimation(value: true);
	}

	private void SetupWidgets(IReadOnlyList<OvertipBuffsPredictionBlockVM.BuffPrediction> buffs)
	{
		int constraintCount = ((buffs.Count <= m_OneColumnLimit) ? m_OneColumnLimit : m_MultipleColumnsLimit);
		m_BuffsGrid.constraintCount = constraintCount;
		for (int i = 0; i < buffs.Count; i++)
		{
			if (i >= m_PooledWidgets.Count)
			{
				BuffPredictionWidget widget = WidgetFactory.GetWidget(m_BuffPrefab, activate: false, strictMatching: true);
				widget.transform.SetParent(m_BuffsGrid.transform, worldPositionStays: false);
				m_PooledWidgets.Add(widget);
			}
			OvertipBuffsPredictionBlockVM.BuffPrediction buffPrediction = buffs[i];
			BuffPredictionWidget buffPredictionWidget = m_PooledWidgets[i];
			buffPredictionWidget.SetIcon(buffPrediction.Icon);
			buffPredictionWidget.SetValue(buffPrediction.ApplyChance);
			buffPredictionWidget.SetActive(isActive: true);
		}
		for (int j = buffs.Count; j < m_PooledWidgets.Count; j++)
		{
			m_PooledWidgets[j].SetActive(isActive: false);
		}
	}

	private void ClearWidgets()
	{
		foreach (BuffPredictionWidget pooledWidget in m_PooledWidgets)
		{
			WidgetFactory.DisposeWidget(pooledWidget);
		}
		m_PooledWidgets.Clear();
	}
}
