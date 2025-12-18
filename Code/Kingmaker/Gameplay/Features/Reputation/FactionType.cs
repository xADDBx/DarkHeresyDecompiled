using System;
using UnityEngine.Serialization;

namespace Kingmaker.Gameplay.Features.Reputation;

public enum FactionType
{
	None,
	[Obsolete]
	Drusians,
	[Obsolete]
	Explorators,
	[Obsolete]
	Kasballica,
	AstraMilitarum,
	AdeptusMechanicus,
	RulingCouncilOfNobles,
	[FormerlySerializedAs("MinistrumAndRedemptionists")]
	MinistorumAndRedemptionists
}
