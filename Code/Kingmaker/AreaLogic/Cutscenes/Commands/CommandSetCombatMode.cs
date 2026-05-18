using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View;
using Kingmaker.View.Equipment;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandSetCombatMode")]
[TypeId("f6e13489adfcaeb4fb61519e0aa9c646")]
public class CommandSetCombatMode : CommandBase
{
	private class Data
	{
		public bool OnSwitchedCalled;
	}

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	public bool InCombat;

	public bool Continuous;

	public bool WaitForAnimation;

	public CommandSignalData OnSwitched = new CommandSignalData
	{
		Name = "On Switched"
	};

	public override bool IsContinuous => Continuous;

	public override bool ShouldHaveControlledUnit => true;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		if (Target.TryGetValue(out var value) && value?.View is UnitEntityView unitEntityView)
		{
			unitEntityView.HandsEquipment.SetCombatVisualState(InCombat);
			return CommandResult.Success;
		}
		return CommandResult.Fail("Cant find unit");
	}

	public override bool TrySkip(CutscenePlayerData player)
	{
		if (base.TrySkip(player))
		{
			return !WaitForAnimation;
		}
		return false;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return OnRun(player, skipping: true);
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		if (IsContinuous)
		{
			return false;
		}
		if (!WaitForAnimation)
		{
			return true;
		}
		UnitViewHandsEquipment unitViewHandsEquipment = (Target.GetValue() as BaseUnitEntity)?.View?.HandsEquipment;
		if (unitViewHandsEquipment != null)
		{
			if (!unitViewHandsEquipment.AreHandsBusyWithAnimation)
			{
				return unitViewHandsEquipment.IsCombatStateConsistent;
			}
			return false;
		}
		return true;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		if (string.IsNullOrEmpty(OnSwitched?.GateId))
		{
			return CommandResult.Success;
		}
		if (!Continuous)
		{
			return CommandResult.Success;
		}
		if (!Target.TryGetValue(out var value))
		{
			return CommandResult.Fail("Failed to find target");
		}
		if (value.AreHandsBusyWithAnimation)
		{
			Data commandData = player.GetCommandData<Data>(this);
			if (!commandData.OnSwitchedCalled)
			{
				commandData.OnSwitchedCalled = true;
				player.SignalGateExtra(OnSwitched.GateId);
			}
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		if (!Target.TryGetValue(out var value))
		{
			return CommandResult.Fail("Failed to find target");
		}
		if (Continuous && InCombat && value.View is UnitEntityView unitEntityView)
		{
			unitEntityView.HandsEquipment.SetCombatVisualState(!InCombat);
		}
		return CommandResult.Success;
	}

	public override CommandSignalData[] GetExtraSignals()
	{
		if (!IsContinuous)
		{
			return base.GetExtraSignals();
		}
		return new CommandSignalData[1] { OnSwitched };
	}

	public override string GetWarning()
	{
		if (!Target || !Target.CanEvaluate())
		{
			return "No unit";
		}
		return null;
	}

	public override string GetCaption()
	{
		if (!Continuous)
		{
			return "Switch <b>combat mode</b> for " + (Target ? Target.GetCaptionShort() : "???");
		}
		return "Set <b>combat mode</b> for " + (Target ? Target.GetCaptionShort() : "???");
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (!Target || !Target.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}
}
