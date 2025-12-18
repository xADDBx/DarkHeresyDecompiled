using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[ComponentName("Actions/PlayCustomMusic")]
[AllowMultipleComponents]
[TypeId("09831a94e6a241b4fa340ea55d486a6d")]
public class PlayCustomMusic : GameAction
{
	[AkEventReference]
	public string MusicEventStart;

	[AkEventReference]
	public string MusicEventStop;

	protected override void RunAction()
	{
	}

	public override string GetCaption()
	{
		return $"Custom music ({MusicEventStart})";
	}
}
