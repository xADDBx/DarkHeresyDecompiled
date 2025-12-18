using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LineOfSightControllerView : View<LineOfSightControllerVM>
{
	[SerializeField]
	private LineOfSightView m_SightPCView;

	protected override void OnBind()
	{
		foreach (LineOfSightVM linesVM in base.ViewModel.LinesVMs)
		{
			DrawLine(linesVM);
		}
		base.ViewModel.LinesVMs.ObserveAdd().Subscribe(delegate(CollectionAddEvent<LineOfSightVM> value)
		{
			DrawLine(value.Value);
		}).AddTo(this);
	}

	private void DrawLine(LineOfSightVM vm)
	{
		LineOfSightView widget = WidgetFactory.GetWidget(m_SightPCView);
		widget.Bind(vm);
		widget.transform.SetParent(base.transform, worldPositionStays: false);
	}
}
