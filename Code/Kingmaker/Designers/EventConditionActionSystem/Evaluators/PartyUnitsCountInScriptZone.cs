using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.MapObjects.SriptZones;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("827a40026f1b73d4cbcb2e50b059e3ec")]
public class PartyUnitsCountInScriptZone : IntEvaluator
{
	[AllowedEntityType(typeof(ScriptZone))]
	public EntityReference ScriptZone;

	public override string GetCaption()
	{
		return $"Count of party units in script zone {ScriptZone}";
	}

	protected override int GetValueInternal()
	{
		ScriptZoneEntity scriptZoneEntity = (ScriptZoneEntity)ScriptZone.FindData();
		if (scriptZoneEntity == null)
		{
			Element.LogError(this, "ScriptZone {0} is missing", ScriptZone);
			return 0;
		}
		int num = 0;
		foreach (ScriptZoneEntity.UnitInfo insideUnit in scriptZoneEntity.InsideUnits)
		{
			BaseUnitEntity baseUnitEntity = insideUnit.Reference.Entity?.ToBaseUnitEntity();
			if (baseUnitEntity != null && baseUnitEntity.Faction.IsPlayer)
			{
				num++;
			}
		}
		return num;
	}
}
