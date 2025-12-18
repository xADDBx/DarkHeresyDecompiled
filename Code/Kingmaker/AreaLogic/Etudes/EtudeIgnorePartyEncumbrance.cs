using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("1aa31cb13856ae648a045db06bf8db47")]
public class EtudeIgnorePartyEncumbrance : EtudeBracketTrigger
{
	protected override void OnEnter()
	{
		Game.Instance.LoadedAreaState.Settings.IgnorePartyEncumbrance.Retain();
	}

	protected override void OnExit()
	{
		Game.Instance.LoadedAreaState.Settings.IgnorePartyEncumbrance.Release();
	}

	protected override void OnResume()
	{
		OnEnter();
	}
}
