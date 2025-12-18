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

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		using (ContextData<PlayerData>.Request().Setup(player))
		{
			Action.Run(ActionList.ExceptionHandlingMode.ThrowAfterListIsComplete);
		}
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
		OnRun(player, skipping: true);
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
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
