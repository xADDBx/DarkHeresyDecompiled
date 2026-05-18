using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.FogOfWar;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[UsedImplicitly]
[ComponentName("Command/CommandSaveFogOfWar")]
[TypeId("d757136f3b84436da1452e6d27a4f0bb")]
public class CommandSaveFogOfWar : CommandBase
{
	public class Data
	{
		public Task Task;

		public byte[] State;

		public FogOfWarArea Area;

		public EntityVisibilityForPlayerController Controller;

		public void Reset()
		{
			State = null;
			Area = null;
			Task = null;
			Controller = null;
		}
	}

	public override bool IsContinuous => true;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		FogOfWarArea active = FogOfWarArea.Active;
		if (active == null)
		{
			commandData.Reset();
			return CommandResult.Fail("No active area");
		}
		try
		{
			commandData.State = null;
			commandData.Area = active;
			commandData.Task = active.RequestData().ContinueWith(delegate(Task<byte[]> task, object state)
			{
				Data data = (Data)state;
				if (task.IsCompletedSuccessfully)
				{
					data.State = task.Result;
				}
				data.Task = null;
			}, commandData);
			commandData.Controller = Game.Instance.Controllers?.EntityVisibilityForPlayerController;
			if (commandData.Controller != null)
			{
				commandData.Controller.DisableVisibleEntityRevealing();
			}
		}
		catch
		{
			commandData.Reset();
			return CommandResult.Fail("Failed to set fog of war");
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		try
		{
			if (commandData.Controller != null)
			{
				commandData.Controller.EnableVisibleEntityRevealing();
			}
			if (commandData.Area != null)
			{
				if (commandData.Task != null)
				{
					commandData.Task.Wait(1000);
				}
				if (commandData.State != null)
				{
					commandData.Area.RestoreFogOfWarMask(commandData.State);
				}
			}
		}
		finally
		{
			commandData.Reset();
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}
}
