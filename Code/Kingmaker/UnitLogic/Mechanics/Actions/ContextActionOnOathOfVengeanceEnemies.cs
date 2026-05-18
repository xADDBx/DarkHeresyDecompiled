using System.Collections.Generic;
using System.Linq;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("537a683dd09f4fc0926480abe3fb1108")]
public class ContextActionOnOathOfVengeanceEnemies : ContextAction
{
	public ActionList Actions;

	public override string GetCaption()
	{
		return "Run actions on all enemies registered for the target ally";
	}

	protected override void RunAction()
	{
		foreach (MechanicEntity item in GetTargetsInternal())
		{
			using (base.Context.PushTarget(item))
			{
				Actions.Run();
			}
		}
	}

	private IEnumerable<MechanicEntity> GetTargetsInternal()
	{
		MechanicEntity caster = base.Context.Caster;
		UnitEntity unitEntity = base.Context.ClickedTarget?.Entity as UnitEntity;
		UnitPartOathOfVengeance unitPartOathOfVengeance = caster?.GetOptional<UnitPartOathOfVengeance>();
		if (caster == null || unitEntity == null || !unitEntity.IsAlly(caster) || unitPartOathOfVengeance == null || !unitPartOathOfVengeance.HasEntries(unitEntity))
		{
			yield break;
		}
		foreach (OathOfVengeanceEntry entry in unitPartOathOfVengeance.GetEntries(unitEntity))
		{
			yield return (UnitEntity)entry.Enemy;
		}
	}

	public IEnumerable<MechanicEntity> GetTargets()
	{
		return GetTargetsInternal().ToArray();
	}
}
