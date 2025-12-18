namespace Kingmaker.Code.Framework.GameLog;

public interface IMergeEventComparer
{
	bool Compare(GameLogEvent evn1, GameLogEvent evn2);
}
