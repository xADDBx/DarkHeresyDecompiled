using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.MapObjects.SriptZones;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[PlayerUpgraderAllowed(true)]
[TypeId("2376c5a54e08498e84849985dd09e8c9")]
public class IsScriptZoneActive : Condition
{
	[AllowedEntityType(typeof(ScriptZone))]
	[ValidateNotEmpty]
	public EntityReference ScriptZone;

	protected override string GetConditionCaption()
	{
		return $"Is script zone active ({ScriptZone})";
	}

	protected override bool CheckCondition()
	{
		if (ScriptZone.FindData() is ScriptZoneEntity scriptZoneEntity)
		{
			return scriptZoneEntity.IsActive;
		}
		return false;
	}
}
