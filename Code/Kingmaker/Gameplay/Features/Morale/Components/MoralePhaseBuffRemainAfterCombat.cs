using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Features.Morale.Components;

[Serializable]
[ClassInfoBox("Система морали оставит этот бафф после боя. Бафф должен использоваться как Heroic или Broken.")]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("efac457482b040d78be703ea117cd2ea")]
public sealed class MoralePhaseBuffRemainAfterCombat : BlueprintComponent
{
}
