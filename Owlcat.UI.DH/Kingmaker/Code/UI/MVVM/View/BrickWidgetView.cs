using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View;

public class BrickWidgetView : BrickBaseView<BrickWidgetVM>
{
	[SerializeField]
	protected WidgetList m_WidgetList;

	[Header("Bricks")]
	[SerializeField]
	private TooltipBricksView m_BricksConfig;

	[SerializeField]
	private TooltipBricksRegistry m_BricksRegistry;

	[FormerlySerializedAs("m_TooltipBrickTextView")]
	[SerializeField]
	private BrickTextView TooltipBrickTextView;

	protected override void OnBind()
	{
		TooltipBrickTextView.Bind(base.ViewModel.TextBrickVM);
		base.ViewModel.Bricks.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}).AddTo(this);
		DrawEntities();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_WidgetList.Clear();
	}

	private void DrawEntities()
	{
		m_WidgetList.Clear();
		bool flag = base.ViewModel.Bricks.Any();
		TooltipBrickTextView.gameObject.SetActive(!flag);
		if (flag)
		{
			TooltipBrickVM tooltipBrickVM = base.ViewModel.Bricks.FirstOrDefault() as TooltipBrickVM;
			MonoBehaviour item = m_BricksRegistry?.GetViewPrefab(tooltipBrickVM?.GetType()) as MonoBehaviour;
			m_WidgetList.DrawMultiEntries(base.ViewModel.Bricks, new List<MonoBehaviour> { item });
		}
	}
}
