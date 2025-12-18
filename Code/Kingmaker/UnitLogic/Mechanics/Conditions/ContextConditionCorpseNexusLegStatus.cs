using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("067220560018437fb386301457023e1b")]
public class ContextConditionCorpseNexusLegStatus : ContextCondition
{
	public bool Master;

	[HideIf("Master")]
	public bool IsDead;

	[ShowIf("Master")]
	public CorpseNexusLegType LegType;

	[ShowIf("Master")]
	public int ExpectedAmount;

	protected override string GetConditionCaption()
	{
		if (!Master)
		{
			return "Checks whether the target is the corpse nexus leg pretending to be dead";
		}
		return "Checks the status of specific corpse nexus legs";
	}

	protected override bool CheckCondition()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		if (maybeCaster == null)
		{
			PFLog.Default.Error("Caster is missing");
			return false;
		}
		MechanicEntity target = base.Target.Entity;
		if (target == null)
		{
			PFLog.Default.Error("Target unit is missing");
			return false;
		}
		if (Master)
		{
			UnitPartCorpseNexusLegs optional = maybeCaster.GetOptional<UnitPartCorpseNexusLegs>();
			if (optional == null)
			{
				return false;
			}
			return optional.Legs.Count((CorpseNexusLegData p) => p.LegType == LegType) == ExpectedAmount;
		}
		return maybeCaster.GetOptional<UnitPartCorpseNexusLegs>()?.Legs.FirstOrDefault((CorpseNexusLegData p) => p.Unit == target)?.PretendDead == IsDead;
	}
}
