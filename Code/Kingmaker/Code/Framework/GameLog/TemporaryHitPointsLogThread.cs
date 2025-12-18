using UnityEngine;

namespace Kingmaker.Code.Framework.GameLog;

public class TemporaryHitPointsLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventTemporaryHitPoints>
{
	public void HandleEvent(GameLogEventTemporaryHitPoints evt)
	{
		string text = ((evt.State == GameLogEventTemporaryHitPoints.States.Add) ? "Add" : "Remove");
		string text2 = ((!evt.Actor.IsNull()) ? evt.Actor.Entity.CharacterName : "???");
		string name = evt.Buff.Name;
		string message = $"%{text} {evt.Amaunt} temporary hit points to {text2}, source buff {name}%";
		AddMessage(new CombatLogMessage(message, Color.black, PrefixIcon.None));
	}
}
