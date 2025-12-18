using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[AllowMultipleComponents]
[ComponentName("SkillCheck/SkillCheckInteractionTrigger")]
[TypeId("7ef57324c2f0f034c8607b74b72edfcd")]
public class SkillCheckInteractionTrigger : EntityFactComponentDelegate, ISkillCheckInteractionTrigger
{
	public ActionList OnSuccess;

	public ActionList OnFailure;

	private void OnInteractInternal(BaseUnitEntity unit, InteractionSkillCheckPart skillCheckInteraction, bool success)
	{
		using (ContextData<MechanicEntityData>.Request().Setup(skillCheckInteraction.Owner))
		{
			using (ContextData<InteractingUnitData>.Request().Setup(unit))
			{
				(success ? OnSuccess : OnFailure).Run();
			}
		}
	}

	void ISkillCheckInteractionTrigger.OnInteract(BaseUnitEntity unit, InteractionSkillCheckPart skillCheckInteraction, bool success)
	{
		OnInteractInternal(unit, skillCheckInteraction, success);
	}
}
