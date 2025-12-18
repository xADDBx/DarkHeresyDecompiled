using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters;

[Serializable]
[TypeId("6381a976b0774843ab2bb51269d7f3fd")]
public sealed class CheckBuffBlueprint : BoolPropertyGetter, PropertyContextAccessor.IOptionalMechanicContext, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public BpRef<BlueprintBuff>[] Blueprints = new BpRef<BlueprintBuff>[0];

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Buff is " + string.Join("|", Blueprints.Dereference().NotNull());
	}

	protected override bool GetBaseValue()
	{
		BlueprintScriptableObject blueprintScriptableObject = this.GetMechanicContext()?.Blueprint;
		BlueprintBuff buff = blueprintScriptableObject as BlueprintBuff;
		if (buff != null)
		{
			return Blueprints.HasItem((BpRef<BlueprintBuff> i) => i == buff);
		}
		return false;
	}
}
