using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class TooltipBrickWidgetView : TooltipBaseBrickView<TooltipBrickWidgetVM>
{
	[SerializeField]
	protected WidgetList m_WidgetList;

	[Header("Bricks")]
	[SerializeField]
	private TooltipBricksView m_BricksConfig;

	[SerializeField]
	private TooltipBrickTextView m_TooltipBrickTextView;

	protected override void OnBind()
	{
		if (base.ViewModel.TooltipBrickTextVM != null)
		{
			m_TooltipBrickTextView.Bind(base.ViewModel.TooltipBrickTextVM);
		}
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
		List<TooltipBaseBrickVM> list = base.ViewModel.CollectBricksVM();
		bool flag = list.Any();
		m_TooltipBrickTextView.gameObject.SetActive(!flag);
		if (flag)
		{
			TooltipBaseBrickVM vm = list.FirstOrDefault();
			MonoBehaviour brickView = TooltipEngine.GetBrickView(m_BricksConfig, vm);
			m_WidgetList.DrawMultiEntries(list, new List<MonoBehaviour> { brickView });
		}
	}
}
