using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Obsolete]
[TypeId("c89848ca6085b104c92d7d4c4509b93d")]
public class WarhammerBlockStarshipComponent : BlueprintComponent
{
	public enum BlockingStrategyType
	{
		Random,
		FromAttackDirection
	}

	public BlockingStrategyType BlockingStrategy;
}
