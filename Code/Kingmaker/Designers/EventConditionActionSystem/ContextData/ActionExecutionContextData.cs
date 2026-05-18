using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.Designers.EventConditionActionSystem.ContextData;

public class ActionExecutionContextData : ContextData<ActionExecutionContextData>
{
	public enum Type
	{
		None,
		Interaction,
		Cutscene,
		Dialog
	}

	public Type ContextType;

	public ActionExecutionContextData Setup(Type type)
	{
		ContextType = type;
		return this;
	}

	protected override void Reset()
	{
		ContextType = Type.None;
	}
}
