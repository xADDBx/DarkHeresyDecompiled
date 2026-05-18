using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.Equipment;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandSetPartyCombatMode")]
[TypeId("7e7a9997cc6e434a80195f135c0d4c78")]
public class CommandSetPartyCombatMode : CommandBase
{
	private class Data
	{
		public bool OnSwitchedCalled;

		public List<float> Delays = new List<float>();

		public List<bool> Applied = new List<bool>();
	}

	public bool InCombat;

	public bool Continuous;

	public bool WaitForAnimation;

	public bool RandomDelay;

	[ShowIf("RandomDelay")]
	public float MaxDelay;

	public CommandSignalData OnSwitched = new CommandSignalData
	{
		Name = "On Switched"
	};

	public override bool IsContinuous => Continuous;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		List<BaseUnitEntity> party = Game.Instance.Player.Party;
		if (skipping)
		{
			return CommandResult.Success;
		}
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Delays = new List<float>(party.Count);
		commandData.Applied = new List<bool>(party.Count);
		for (int i = 0; i < party.Count; i++)
		{
			commandData.Delays.Add(RandomDelay ? player.Random.Range(0f, MaxDelay) : 0f);
			commandData.Applied.Add(item: false);
		}
		return CommandResult.Success;
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
		List<BaseUnitEntity> party = Game.Instance.Player.Party;
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Applied.Clear();
		for (int i = 0; i < party.Count; i++)
		{
			party[i]?.View?.HandsEquipment.SetCombatVisualState(InCombat);
			commandData.Applied.Add(item: true);
		}
		return CommandResult.Success;
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
		List<bool> applied = player.GetCommandData<Data>(this).Applied;
		if (applied == null || !applied.All((bool x) => x))
		{
			return false;
		}
		if (!WaitForAnimation)
		{
			return true;
		}
		List<BaseUnitEntity> party = Game.Instance.Player.Party;
		bool result = true;
		foreach (BaseUnitEntity item in party)
		{
			UnitViewHandsEquipment unitViewHandsEquipment = item?.View?.HandsEquipment;
			if (unitViewHandsEquipment != null && ((bool)unitViewHandsEquipment.AreHandsBusyWithAnimation || !unitViewHandsEquipment.IsCombatStateConsistent))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		List<BaseUnitEntity> party = Game.Instance.Player.Party;
		Data commandData = player.GetCommandData<Data>(this);
		for (int i = 0; i < party.Count; i++)
		{
			BaseUnitEntity baseUnitEntity = party[i];
			if (commandData.Applied.Count > i && !commandData.Applied[i] && (commandData.Delays.Count <= i || (commandData.Delays.Count > i && time >= (double)commandData.Delays[i])))
			{
				baseUnitEntity?.View?.HandsEquipment.SetCombatVisualState(InCombat);
				commandData.Applied[i] = true;
			}
		}
		if (commandData.Applied.All((bool x) => x) && !commandData.OnSwitchedCalled && !string.IsNullOrEmpty(OnSwitched.GateId))
		{
			commandData.OnSwitchedCalled = true;
			player.SignalGateExtra(OnSwitched.GateId);
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if (item != null && Continuous && InCombat)
			{
				item?.View?.HandsEquipment.SetCombatVisualState(!InCombat);
			}
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

	public override string GetCaption()
	{
		if (!Continuous)
		{
			return $"Switch <b>combat mode</b> for whole Party to {InCombat}";
		}
		return $"Set <b>combat mode</b> for whole Party to {InCombat}";
	}
}
