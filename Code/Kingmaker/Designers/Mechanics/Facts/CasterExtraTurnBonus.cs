using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("69961325deb248f78eac3cdc2fba755e")]
public class CasterExtraTurnBonus : UnitFactComponentDelegate
{
	public ContextValue ActionPointsBonus;

	public ContextValue MovementPointsBonus;

	public ActionList ActionsOnTarget;

	public bool OnlyIfTargetIsNotOwner = true;
}
