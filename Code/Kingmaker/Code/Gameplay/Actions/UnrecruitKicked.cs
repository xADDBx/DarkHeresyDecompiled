using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Actions;

[ComponentName("Actions/UnrecruitKicked")]
[PlayerUpgraderAllowed(false)]
[AllowMultipleComponents]
[TypeId("0b279b33118542d995cb2289064a4e1c")]
public class UnrecruitKicked : Unrecruit
{
	protected override CompanionExState ExState => CompanionExState.Kicked;

	public override string GetCaption()
	{
		return "Unrecruit Kicked (" + base.CaptionName + ")";
	}
}
