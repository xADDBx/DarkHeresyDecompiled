using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Actions;

[ComponentName("Actions/UnrecruitDead")]
[PlayerUpgraderAllowed(false)]
[AllowMultipleComponents]
[TypeId("5429257128e642cd8fd998bfbc536855")]
public class UnrecruitDead : Unrecruit
{
	protected override CompanionExState ExState => CompanionExState.Dead;

	public override string GetCaption()
	{
		return "Unrecruit Dead (" + base.CaptionName + ")";
	}
}
