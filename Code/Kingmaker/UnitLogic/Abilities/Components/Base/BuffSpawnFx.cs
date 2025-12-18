using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[AllowedOn(typeof(BlueprintBuff))]
[AllowMultipleComponents]
[ComponentName("FX/BuffSpawnFx")]
[TypeId("3969212ace044208a60899dc4e1f3b3e")]
public class BuffSpawnFx : AbilitySpawnFx
{
	public bool DestroyOnDeAttach = true;
}
