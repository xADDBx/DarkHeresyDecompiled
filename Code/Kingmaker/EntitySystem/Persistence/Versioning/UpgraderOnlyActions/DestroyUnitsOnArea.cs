using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Persistence.Versioning.UpgraderOnlyActions;

[TypeId("ca673892f4284c4ea5cd7b9219a800d0")]
public class DestroyUnitsOnArea : PlayerUpgraderOnlyAction
{
	public BlueprintUnitReference UnitBlueprint;

	public override string GetCaption()
	{
		return "Удаляет всех юнитов с Ареи с указанным БП";
	}

	protected override void RunActionOverride()
	{
		foreach (AbstractUnitEntity item in Game.Instance.EntityPools.AllUnits.Where((AbstractUnitEntity u) => u.Blueprint == UnitBlueprint.Get()))
		{
			Game.Instance.Controllers.EntityDestroyer.Destroy(item);
		}
	}
}
