using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Buffs.Actions;

[Obsolete]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("61db6c10a2573084fbc97f33006da3b2")]
public abstract class BuffAction : ContextAction
{
}
