using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Interaction;
using Newtonsoft.Json;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
public class EtudeBracketOverrideUnitInteraction : IUnitInteraction
{
	[JsonProperty]
	public readonly IEtudeBracketOverrideInteraction Source;

	public int Distance => Source.Distance;

	public int ActionCost => 0;

	public bool IsApproach => false;

	public float ApproachCooldown => 5f;

	public bool MainPlayerPreferred => true;

	public bool IsDialog => Source.IsDialog;

	public bool AllowInCombat => Source.AllowInCombat;

	public bool AllowWithHelpless => false;

	public LocalizedString DisplayName => null;

	[JsonConstructor]
	protected EtudeBracketOverrideUnitInteraction()
	{
	}

	public EtudeBracketOverrideUnitInteraction(IEtudeBracketOverrideInteraction source)
	{
		Source = source;
	}

	public AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
	{
		return Source.Interact(user, target);
	}

	public bool IsAvailable(BaseUnitEntity initiator, AbstractUnitEntity target)
	{
		return true;
	}
}
