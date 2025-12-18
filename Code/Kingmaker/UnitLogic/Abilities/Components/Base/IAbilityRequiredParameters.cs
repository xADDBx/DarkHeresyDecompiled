using System;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[Obsolete]
public interface IAbilityRequiredParameters
{
	AbilityParameter RequiredParameters { get; }
}
