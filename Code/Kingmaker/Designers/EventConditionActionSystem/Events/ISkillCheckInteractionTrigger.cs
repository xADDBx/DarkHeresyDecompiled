using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.MapObjects;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[Obsolete]
public interface ISkillCheckInteractionTrigger
{
	void OnInteract(BaseUnitEntity unit, InteractionSkillCheckPart skillCheckInteraction, bool success);
}
