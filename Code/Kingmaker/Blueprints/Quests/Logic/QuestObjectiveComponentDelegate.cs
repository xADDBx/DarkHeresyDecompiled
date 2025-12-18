using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints.Quests.Logic;

[AllowedOn(typeof(BlueprintQuestObjective))]
[TypeId("a175e554d3ec9624f8f4075ca17036dd")]
public abstract class QuestObjectiveComponentDelegate : EntityFactComponentDelegate<QuestBook>, INodeEditorDescriptionProvider
{
	protected QuestObjective Objective => (QuestObjective)base.Fact;

	public abstract string GetDescription();
}
