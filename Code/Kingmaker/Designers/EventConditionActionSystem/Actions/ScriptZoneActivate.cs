using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.MapObjects.SriptZones;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/ScriptZoneActivate")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("c99aa15b0ad07ce4db8044f7adfccaa5")]
public class ScriptZoneActivate : GameAction
{
	[AllowedEntityType(typeof(ScriptZone))]
	[ValidateNotEmpty]
	public EntityReference ScriptZone;

	protected override void RunAction()
	{
		if (ScriptZone.FindData() is ScriptZoneEntity scriptZoneEntity)
		{
			scriptZoneEntity.IsActive = true;
		}
	}

	public override string GetCaption()
	{
		return $"Script Zone Activate ({ScriptZone})";
	}
}
