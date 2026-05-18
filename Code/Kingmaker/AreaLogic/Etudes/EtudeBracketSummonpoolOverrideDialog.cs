using System;
using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("eca8bd91e08734243945547d3d64dffd")]
public class EtudeBracketSummonpoolOverrideDialog : EtudeBracketOverrideInteraction
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public EtudeBracketOverrideUnitInteraction Interaction;
	}

	[SerializeField]
	[FormerlySerializedAs("SummonPool")]
	private BlueprintSummonPoolReference m_SummonPool;

	public BlueprintDialogReference Dialog;

	public BlueprintSummonPool SummonPool => m_SummonPool?.Get();

	public override bool IsDialog => true;

	protected override void OnEnter()
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		componentData.Interaction = new EtudeBracketOverrideUnitInteraction(this);
		ISummonPool summonPool = Game.Instance.SummonPools.Get(SummonPool);
		if (summonPool == null)
		{
			return;
		}
		foreach (AbstractUnitEntity unit in summonPool.Units)
		{
			unit.GetOrCreate<PartUnitInteractions>().AddInteraction(componentData.Interaction);
		}
	}

	protected override void OnExit()
	{
		ISummonPool summonPool = Game.Instance.SummonPools.Get(SummonPool);
		if (summonPool == null)
		{
			return;
		}
		ComponentData componentData = RequestTransientData<ComponentData>();
		foreach (AbstractUnitEntity unit in summonPool.Units)
		{
			unit.GetOrCreate<PartUnitInteractions>().RemoveInteraction(componentData.Interaction);
		}
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
