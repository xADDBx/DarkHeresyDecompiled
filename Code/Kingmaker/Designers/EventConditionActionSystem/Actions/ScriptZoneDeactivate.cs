using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.MapObjects.SriptZones;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/ScriptZoneDeactivate")]
[AllowMultipleComponents]
[TypeId("936004d258436d2459d0339955a70892")]
[PlayerUpgraderAllowed(true)]
public class ScriptZoneDeactivate : GameAction
{
	[AllowedEntityType(typeof(ScriptZone))]
	[ValidateNotEmpty]
	public EntityReference ScriptZone;

	protected override void RunAction()
	{
		if (ScriptZone.FindData() is ScriptZoneEntity scriptZoneEntity)
		{
			scriptZoneEntity.IsActive = false;
		}
	}

	public override string GetCaption()
	{
		return $"Script Zone Deactivate ({ScriptZone})";
	}
}
