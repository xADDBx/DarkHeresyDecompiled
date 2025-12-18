using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints.Quests.Logic;

[AllowedOn(typeof(BlueprintQuest))]
[TypeId("f12a47c527df9684f81b0d31f42b1b9c")]
public class QuestComponentDelegate : EntityFactComponentDelegate<QuestBook>
{
	protected Quest Quest => (Quest)base.Fact;
}
