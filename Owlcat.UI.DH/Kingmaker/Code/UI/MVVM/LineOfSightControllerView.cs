using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LineOfSightControllerView : View<LineOfSightControllerVM>
{
	[SerializeField]
	private LineOfSightView m_SightPCView;

	protected override void OnBind()
	{
	}

	private void DrawLine(LineOfSightVM vm)
	{
	}
}
