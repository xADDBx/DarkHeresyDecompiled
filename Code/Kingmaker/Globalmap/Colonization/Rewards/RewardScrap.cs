using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[Obsolete]
[TypeId("45c3f4edb1df41d7b2cc685d71943755")]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[AllowedOn(typeof(BlueprintAnswer))]
public class RewardScrap : Reward
{
	public int Scrap;
}
