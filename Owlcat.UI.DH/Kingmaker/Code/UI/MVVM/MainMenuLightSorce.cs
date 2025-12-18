using System;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[Serializable]
public class MainMenuLightSorce
{
	public Light LightSorce;

	[HideInInspector]
	public LightTweenAnchor CurrentTween;

	[HideInInspector]
	public LightTweenAnchor PrevTween;

	public LightTweenAnchor StartAnchor;

	public LightTweenAnchor EndAnchor;
}
