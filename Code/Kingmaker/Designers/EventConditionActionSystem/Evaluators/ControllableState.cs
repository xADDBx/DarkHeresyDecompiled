using Kingmaker.AreaLogic.SceneControllables;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("19adcd35ecaa43cdbc3b028092388994")]
public class ControllableState : IntEvaluator
{
	public ControllableReference IdOfObject;

	public override string GetCaption()
	{
		return "State of controllable " + IdOfObject.EntityNameInEditor;
	}

	protected override int GetValueInternal()
	{
		if (!Game.Instance.Controllers.SceneControllables.TryGetState(IdOfObject.UniqueId, out var state))
		{
			return 0;
		}
		if (!state.State.HasValue)
		{
			return 0;
		}
		return state.State.Value;
	}
}
