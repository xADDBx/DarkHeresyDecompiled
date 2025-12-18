using System;
using System.Collections.Generic;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[Serializable]
public class TransitionMapPart
{
	public BlueprintMultiEntrance.BlueprintMultiEntranceMap Map;

	public GameObject MapObject;

	public List<TransitionEntryBaseView> Entries;

	public WidgetList WidgetList;

	public RectTransform LightBeam;

	public CanvasGroup LightBeamCanvas;

	public List<PointOnMap> PointsOnMap;

	public bool CustomPantographMaxY;

	[ShowIf("CustomPantographMaxY")]
	public float CustomPantographMaxYValue;

	public OwlcatButton Close;

	public void Initialize()
	{
		MapObject.SetActive(value: false);
		Entries.ForEach(delegate(TransitionEntryBaseView v)
		{
			v.Initialize();
		});
	}
}
