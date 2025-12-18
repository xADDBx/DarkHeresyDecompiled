using System;
using JetBrains.Annotations;

namespace Kingmaker.UnitLogic.Commands;

[Obsolete]
public sealed class UnitOverwatchAttack : UnitUseAbilityAbstract<UnitOverwatchAttackParams>
{
	public UnitOverwatchAttack([NotNull] UnitOverwatchAttackParams @params)
		: base(@params)
	{
	}
}
