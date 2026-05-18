using System;
using System.Collections.Generic;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[Serializable]
public class TransitionMapPart_OBSOLETE
{
	public OwlcatButton Close;

	public BlueprintMultiEntranceMap Map;

	public GameObject MapObject;

	public List<TransitionEntryBaseView_OBSOLETE> Entries;

	public WidgetList WidgetList;

	public RectTransform LightBeam;

	public CanvasGroup LightBeamCanvas;

	public List<PointOnMap_OBSOLETE> PointsOnMap;

	public bool CustomPantographMaxY;

	[ShowIf("CustomPantographMaxY")]
	public float CustomPantographMaxYValue;

	public void Initialize()
	{
		MapObject.SetActive(value: false);
		Entries.ForEach(delegate(TransitionEntryBaseView_OBSOLETE v)
		{
			v.Initialize();
		});
	}
}
