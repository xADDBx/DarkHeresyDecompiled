using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Obsolete]
[TypeId("41cb587881aa4b145bd090532a08dac1")]
public class FactTarget : UnitFactComponentDelegate
{
	public enum TargetType
	{
		SkillChecker,
		Party,
		Starship,
		MainCharacter
	}

	public TargetType Target;
}
