using Kingmaker.Blueprints;
using Kingmaker.Designers;
using Kingmaker.View.Spawners;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.EntitySystem.Persistence.Versioning.UpgraderOnlyActions;

[TypeId("5091aab5196133c489dda4066af8d1fc")]
internal class RespawnNewUnit : PlayerUpgraderOnlyAction
{
	[InfoBox("Don't call this action if you dont understand what it's for")]
	[AllowedEntityType(typeof(UnitSpawnerBase))]
	public EntityReference Spawner;

	protected override void RunActionOverride()
	{
		GameHelper.GetUnitSpawner(Spawner).ForceReSpawn();
	}

	public override string GetCaption()
	{
		return "RespawnNewUnit " + Spawner?.EntityNameInEditor;
	}

	public override string GetDescription()
	{
		return "RespawnNewUnit " + Spawner?.EntityNameInEditor;
	}
}
