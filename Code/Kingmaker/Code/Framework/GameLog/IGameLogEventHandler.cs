namespace Kingmaker.Code.Framework.GameLog;

public interface IGameLogEventHandler<in T> where T : GameLogEvent
{
	void HandleEvent(T evt);
}
