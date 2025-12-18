using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("e9147d1aec3425b47bede9b793a9ffa5")]
public class EtudeIgnorePersonalEncumbrance : EtudeBracketTrigger
{
	protected override void OnEnter()
	{
		Game.Instance.LoadedAreaState.Settings.IgnorePersonalEncumbrance.Retain();
	}

	protected override void OnExit()
	{
		Game.Instance.LoadedAreaState.Settings.IgnorePersonalEncumbrance.Release();
	}

	protected override void OnResume()
	{
		OnEnter();
	}
}
