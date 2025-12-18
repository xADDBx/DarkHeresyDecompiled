using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Designers.WarhammerSurfaceCombatPrototype;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("bf4bdb1263684ce08f9f864df4836ac7")]
public class ContextActionRunPsychicPhenomena : ContextAction
{
	public bool UsePerilsEffect;

	private BlueprintPsykerRoot PsykerRoot => ConfigRoot.Instance.PsykerRoot;

	public override string GetCaption()
	{
		return "Run psychic phenomena";
	}

	protected override void RunAction()
	{
		BlueprintPsykerRoot.PhenomenaData phenomena = (UsePerilsEffect ? PsykerRoot.PerilsOfTheWarp : PsykerRoot.PsychicPhenomena).Random(PFStatefulRandom.Mechanics);
		PsychicPhenomenaController.TriggerPsychicPhenomenaForced(base.TargetEntity, base.Context, phenomena, UsePerilsEffect);
	}
}
