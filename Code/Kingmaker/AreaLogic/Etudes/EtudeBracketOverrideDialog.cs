using System;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("8b4c3a5898712654596bc06311e73737")]
public class EtudeBracketOverrideDialog : EtudeBracketOverrideInteraction
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public EtudeBracketOverrideUnitInteraction Interaction;
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public BlueprintDialogReference Dialog;

	public override bool IsDialog => true;

	protected override void OnEnter()
	{
		PartUnitInteractions orCreate = Unit.GetValue().GetOrCreate<PartUnitInteractions>();
		ComponentData componentData = RequestTransientData<ComponentData>();
		componentData.Interaction = new EtudeBracketOverrideUnitInteraction(this);
		orCreate.AddInteraction(componentData.Interaction);
	}

	protected override void OnExit()
	{
		PartUnitInteractions orCreate = Unit.GetValue().GetOrCreate<PartUnitInteractions>();
		ComponentData componentData = RequestTransientData<ComponentData>();
		orCreate.RemoveInteraction(componentData.Interaction);
	}

	protected override void OnResume()
	{
		OnEnter();
	}

	public override AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
	{
		if (!(target is BaseUnitEntity unit))
		{
			return AbstractUnitCommand.ResultType.Fail;
		}
		DialogData data = DialogController.SetupDialogWithUnit((BlueprintDialog)Dialog.GetBlueprint(), unit, user);
		Game.Instance.Controllers.DialogController.StartDialog(data);
		return AbstractUnitCommand.ResultType.Success;
	}
}
