using Kingmaker.RuleSystem;

namespace Kingmaker.Code.Framework.GameLog;

public interface IGameLogRuleHandler<in T> where T : RulebookEvent
{
	void HandleEvent(T evt);
}
