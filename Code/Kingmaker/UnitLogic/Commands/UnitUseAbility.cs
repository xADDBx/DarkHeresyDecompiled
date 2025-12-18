using JetBrains.Annotations;

namespace Kingmaker.UnitLogic.Commands;

public class UnitUseAbility : UnitUseAbilityAbstract<UnitUseAbilityParams>
{
	public UnitUseAbility([NotNull] UnitUseAbilityParams @params)
		: base(@params)
	{
	}
}
