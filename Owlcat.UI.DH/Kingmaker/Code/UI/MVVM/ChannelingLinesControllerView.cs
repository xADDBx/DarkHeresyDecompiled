using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ChannelingLinesControllerView : View<ChannelingLinesControllerVM>
{
	[SerializeField]
	private ChannelingLineView m_LineView;

	private List<ChannelingLineView> m_ActiveLines = new List<ChannelingLineView>();

	protected override void OnBind()
	{
		foreach (ChannelingLineVM linesVM in base.ViewModel.LinesVMs)
		{
			DrawLine(linesVM);
		}
		base.ViewModel.ObserveLineAdded().Subscribe(delegate(CollectionAddEvent<ChannelingLineVM> value)
		{
			DrawLine(value.Value);
		}).AddTo(this);
		base.ViewModel.ObserveLineRemoved().Subscribe(delegate(CollectionRemoveEvent<ChannelingLineVM> value)
		{
			RemoveLine(value.Value);
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		foreach (ChannelingLineView activeLine in m_ActiveLines)
		{
			WidgetFactory.DisposeWidget(activeLine);
		}
		m_ActiveLines.Clear();
	}

	private void RemoveLine(ChannelingLineVM valueValue)
	{
		ChannelingLineView channelingLineView = m_ActiveLines.FirstOrDefault((ChannelingLineView l) => l.ViewModel == valueValue);
		if (!(channelingLineView == null))
		{
			m_ActiveLines.Remove(channelingLineView);
			WidgetFactory.DisposeWidget(channelingLineView);
		}
	}

	private void DrawLine(ChannelingLineVM vm)
	{
		ChannelingLineView widget = WidgetFactory.GetWidget(m_LineView);
		widget.Bind(vm);
		widget.transform.SetParent(base.transform, worldPositionStays: false);
		m_ActiveLines.Add(widget);
	}
}
