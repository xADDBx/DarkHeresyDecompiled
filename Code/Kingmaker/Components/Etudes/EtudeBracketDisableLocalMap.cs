using System;
using Kingmaker.AreaLogic.Etudes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Components.Etudes;

[Serializable]
[TypeId("c30236221dc948e79bb8d1804b5e14d3")]
public class EtudeBracketDisableLocalMap : EtudeBracketTrigger
{
	public override bool RequireLinkedArea => true;

	protected override void OnEnter()
	{
		Game.Instance.LoadedAreaState.Settings.DisableLocalMap.Retain();
	}

	protected override void OnResume()
	{
		Game.Instance.LoadedAreaState.Settings.DisableLocalMap.Retain();
	}

	protected override void OnExit()
	{
		Game.Instance.LoadedAreaState.Settings.DisableLocalMap.Release();
	}
}
