using System;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[Serializable]
public class LocalMapMarkerSet
{
	public LocalMapMarkType Type;

	public LocalMapMarkerPCView View;

	public RectTransform Container;
}
