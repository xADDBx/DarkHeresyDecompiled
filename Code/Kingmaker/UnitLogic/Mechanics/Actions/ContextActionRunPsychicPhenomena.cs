using Kingmaker.Code.AreaLogic;
using Kingmaker.Controllers;
using Kingmaker.Designers.WarhammerSurfaceCombatPrototype;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("bf4bdb1263684ce08f9f864df4836ac7")]
public class ContextActionRunPsychicPhenomena : ContextAction
{
	public bool UsePerilsEffect;

	public override string GetCaption()
	{
		return "Run psychic phenomena";
	}

	protected override void RunAction()
	{
		PartVeil partVeil = Game.Instance.LoadedArea?.Veil;
		if (partVeil != null)
		{
			BlueprintPsykerRoot.PhenomenaData phenomenaData = PhenomenaListResolver.SelectWeighted(UsePerilsEffect ? partVeil.GetResolvedPerils() : partVeil.GetResolvedPhenomena(), PFStatefulRandom.Mechanics);
			if (phenomenaData != null)
			{
				PsychicPhenomenaController.TriggerPsychicPhenomenaForced(base.TargetEntity, base.Context, phenomenaData, UsePerilsEffect);
			}
		}
	}
}
