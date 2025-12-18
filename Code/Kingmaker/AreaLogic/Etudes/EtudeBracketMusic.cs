using System;
using Kingmaker.Sound;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("6757a9d4bdbafcf4181a5af073f2c14f")]
public class EtudeBracketMusic : EtudeBracketTrigger
{
	public AkEventReference StartTheme;

	public AkEventReference StopTheme;

	public override bool RequireLinkedArea => true;

	protected override void OnExit()
	{
		LogChannel.Audio.Log("Stop etude music " + this);
	}

	protected override void OnEnter()
	{
		LogChannel.Audio.Log("Play etude music " + this);
	}

	protected override void OnResume()
	{
		LogChannel.Audio.Log("Resume etude music " + this);
	}
}
