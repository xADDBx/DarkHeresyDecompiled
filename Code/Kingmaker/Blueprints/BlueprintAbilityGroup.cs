using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;

namespace Kingmaker.Blueprints;

[HashRoot]
[ComponentName("Ability/BlueprintAbilityGroup")]
[TypeId("84a976c8e48e6274e8367073fad4a237")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintAbilityGroup : BlueprintScriptableObject
{
	public int CooldownInRounds;

	public bool IsWeaponAttackGroup;
}
