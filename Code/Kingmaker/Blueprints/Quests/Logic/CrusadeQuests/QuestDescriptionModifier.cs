using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints.Quests.Logic.CrusadeQuests;

[AllowedOn(typeof(BlueprintQuest))]
[AllowMultipleComponents]
[TypeId("c544d112d5664976bfa5903c39e8c530")]
public abstract class QuestDescriptionModifier : QuestComponentDelegate
{
	public abstract string Modify(string originalString);

	public abstract bool IsComplete();

	public abstract bool IsFailed();
}
