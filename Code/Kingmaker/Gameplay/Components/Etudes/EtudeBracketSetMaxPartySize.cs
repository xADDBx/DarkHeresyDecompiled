using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Gameplay.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Components.Etudes;

[TypeId("d866b0da6fb3001459c059bbb4371300")]
public class EtudeBracketSetMaxPartySize : EtudeBracketTrigger
{
	[SerializeField]
	public int MaxPartySize = 6;

	protected override void OnEnter()
	{
		Game.Instance.Player.GetOrCreate<PartMaxPartySize>().Register(EtudeBracketTrigger.Etude, this);
	}

	protected override void OnResume()
	{
		Game.Instance.Player.GetOrCreate<PartMaxPartySize>().Register(EtudeBracketTrigger.Etude, this);
	}

	protected override void OnExit()
	{
		Game.Instance.Player.GetOptional<PartMaxPartySize>()?.Unregister(EtudeBracketTrigger.Etude, this);
	}
}
