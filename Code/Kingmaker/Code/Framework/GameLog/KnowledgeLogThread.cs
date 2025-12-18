using System.Text;
using Kingmaker.Inspect;

namespace Kingmaker.Code.Framework.GameLog;

public class KnowledgeLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventKnowledge>
{
	private readonly StringBuilder m_Parts = new StringBuilder();

	public void HandleEvent(GameLogEventKnowledge evt)
	{
	}

	private void CheckIsUnlocked(InspectUnitsManager.UnitInfo unitInfo, UnitInfoPart part)
	{
	}
}
