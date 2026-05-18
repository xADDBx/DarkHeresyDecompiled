using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.MapObjects.SriptZones;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("31dd684969ea477393d08b136c0d5d3f")]
public class UnitsCountInScriptZone : IntEvaluator
{
	[AllowedEntityType(typeof(ScriptZone))]
	public EntityReference ScriptZone;

	public override string GetCaption()
	{
		return $"Count of units in script zone {ScriptZone}";
	}

	protected override int GetValueInternal()
	{
		ScriptZoneEntity scriptZoneEntity = (ScriptZoneEntity)ScriptZone.FindData();
		if (scriptZoneEntity == null)
		{
			Element.LogError(this, "ScriptZone {0} is missing", ScriptZone);
			return 0;
		}
		return scriptZoneEntity.InsideUnits.Count;
	}
}
