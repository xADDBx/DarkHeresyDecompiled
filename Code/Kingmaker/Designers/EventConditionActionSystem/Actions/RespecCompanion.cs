using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("e7a9602018fa91a408db2efd7ca131e2")]
public class RespecCompanion : GameAction
{
	public bool ForFree;

	public bool MatchPlayerXpExactly;

	public override string GetCaption()
	{
		return "Respecialize companion";
	}

	protected override void RunAction()
	{
		Player player = Game.Instance.Player;
		List<BaseUnitEntity> respecUnits = (from ch in player.AllCrossSceneUnits.Where(delegate(BaseUnitEntity u)
			{
				UnitPartCompanion optional = u.GetOptional<UnitPartCompanion>();
				if (optional == null || optional.State != CompanionState.InParty)
				{
					UnitPartCompanion optional2 = u.GetOptional<UnitPartCompanion>();
					if (optional2 == null)
					{
						return false;
					}
					return optional2.State == CompanionState.Remote;
				}
				return true;
			})
			where PartUnitProgression.CanRespec(ch)
			select ch).ToList();
		_ = ForFree;
		if (UtilityNet.IsControlMainCharacter())
		{
			EventBus.RaiseEvent(delegate(ICharacterSelectorHandler h)
			{
				h.HandleSelectCharacter(respecUnits, FinishRespecialization);
			});
		}
	}

	public void FinishRespecialization(BaseUnitEntity unit)
	{
		Game.Instance.GameCommandQueue.FinishRespec(unit, ForFree);
	}
}
