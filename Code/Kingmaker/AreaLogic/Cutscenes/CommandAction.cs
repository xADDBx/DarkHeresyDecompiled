using System;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Cutscenes;

[ComponentName("Command/CommandAction")]
[TypeId("a07b97ed760cca9409b22e2e3ebc622f")]
public class CommandAction : CommandBase
{
	public class PlayerData : ContextData<PlayerData>
	{
		public CutscenePlayerData Player { get; private set; }

		public PlayerData Setup(CutscenePlayerData player)
		{
			Player = player;
			return this;
		}

		protected override void Reset()
		{
			Player = null;
		}
	}

	public ActionList Action;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		using (ContextData<PlayerData>.Request().Setup(player))
		{
			try
			{
				Action.Run(ActionList.ExceptionHandlingMode.ThrowAfterListIsComplete);
			}
			catch (AggregateException ex)
			{
				return CommandResult.FromException(ex.InnerException);
			}
			catch (Exception e)
			{
				return CommandResult.FromException(e);
			}
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return OnRun(player, skipping: true);
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override string GetCaption()
	{
		if (Action == null || Action.Actions == null)
		{
			return "<b>Action</b> none";
		}
		GameAction gameAction = Action.Actions.FirstOrDefault();
		if (gameAction != null)
		{
			return gameAction.GetCaption() + ((Action.Actions.Length > 1) ? (" and " + (Action.Actions.Length - 1) + " more") : "");
		}
		return "none";
	}
}
