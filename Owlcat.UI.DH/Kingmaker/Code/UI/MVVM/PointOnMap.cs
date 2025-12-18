using System;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

[Serializable]
public class PointOnMap
{
	public string Comment;

	public CanvasTransformSettings LightBeamPointSettings;

	public OwlcatMultiButton PointButton;
}
