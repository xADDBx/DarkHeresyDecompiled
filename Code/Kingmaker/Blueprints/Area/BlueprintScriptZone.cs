using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.Mechanics.Entities;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints.Area;

[TypeId("2e95eea1aa90d2b428dfe389189dc287")]
public class BlueprintScriptZone : BlueprintMapObject
{
	[ValidateNotNull]
	public ConditionsChecker TriggerConditions;

	[ValidateNotNull]
	public ActionList EnterActions;

	[ValidateNotNull]
	public ActionList ExitActions;

	protected override Type GetFactType()
	{
		return typeof(EntityFact<ScriptZoneEntity>);
	}
}
