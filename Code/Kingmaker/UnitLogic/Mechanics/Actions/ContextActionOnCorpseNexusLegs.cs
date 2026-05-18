using System.Collections.Generic;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("a344f3b729c14c788ad47144be2e4bc1")]
public class ContextActionOnCorpseNexusLegs : ContextAction
{
	[HideIf("OnLivingLegs")]
	public bool OnDeadLegs;

	[HideIf("OnDeadLegs")]
	public bool OnLivingLegs;

	public ActionList Actions;

	public override string GetCaption()
	{
		return "Run actions on Corpse Nexus legs";
	}

	protected override void RunAction()
	{
		UnitPartCorpseNexusLegs unitPartCorpseNexusLegs = base.Context.Caster?.GetOptional<UnitPartCorpseNexusLegs>();
		if (unitPartCorpseNexusLegs == null)
		{
			return;
		}
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		foreach (CorpseNexusLegData leg in unitPartCorpseNexusLegs.Legs)
		{
			if ((!OnDeadLegs && !OnLivingLegs) || (OnDeadLegs && leg.PretendDead) || (OnLivingLegs && !leg.PretendDead))
			{
				list.Add(leg.Unit);
			}
		}
		foreach (BaseUnitEntity item in list)
		{
			using (base.Context.PushTarget(item))
			{
				Actions.Run();
			}
		}
	}
}
