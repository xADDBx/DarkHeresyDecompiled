using Code.Framework.Utility.UnityExtensions;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.GuidUtility;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandIgnoreCombat")]
[TypeId("733a21023bf2444db3f696a212b80ee6")]
public class CommandIgnoreCombat : CommandBase
{
	private class Data
	{
		public UnitReference Unit;

		[CanBeNull]
		public string OriginalGroupId;
	}

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public bool KeepWeaponsOut;

	public override bool IsContinuous => true;

	public override bool ShouldHaveControlledUnit => true;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		if (!Unit.TryGetValue(out var value) || !(value is BaseUnitEntity baseUnitEntity))
		{
			return CommandResult.Fail("Unit not found");
		}
		Data commandData = player.GetCommandData<Data>(this);
		if ((commandData.Unit = baseUnitEntity.FromBaseUnitEntity()) == null)
		{
			return CommandResult.Fail("Unit not found");
		}
		if (KeepWeaponsOut)
		{
			baseUnitEntity.View.HandsEquipment.SetCombatVisualState(inCombat: true);
		}
		if (baseUnitEntity.CombatGroup.Count > 1)
		{
			commandData.OriginalGroupId = baseUnitEntity.CombatGroup.Id;
			baseUnitEntity.CombatGroup.Id = Uuid.Instance.CreateString();
		}
		baseUnitEntity.Features.IsUntargetable.Retain();
		baseUnitEntity.Features.IsIgnoredByCombat.Retain();
		baseUnitEntity.Passive.Retain();
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData.Unit == null)
		{
			return CommandResult.Fail("Unit not found");
		}
		BaseUnitEntity baseUnitEntity = commandData.Unit.Entity.ToBaseUnitEntity();
		if (!commandData.OriginalGroupId.IsNullOrEmpty())
		{
			baseUnitEntity.CombatGroup.Id = commandData.OriginalGroupId;
		}
		baseUnitEntity.Features.IsUntargetable.Release();
		baseUnitEntity.Features.IsIgnoredByCombat.Release();
		baseUnitEntity.Passive.Release();
		if (KeepWeaponsOut)
		{
			baseUnitEntity.View.HandsEquipment.SetCombatVisualState(inCombat: false);
		}
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (!Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}

	public override string GetCaption()
	{
		return Unit?.GetCaptionShort() + " ignore combat";
	}
}
