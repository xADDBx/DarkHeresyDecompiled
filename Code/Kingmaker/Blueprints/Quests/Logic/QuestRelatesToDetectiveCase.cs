using Kingmaker.Blueprints.Attributes;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Quests.Logic;

[AllowedOn(typeof(BlueprintQuest))]
[TypeId("0050993431d64665a573d14fa4858074")]
public class QuestRelatesToDetectiveCase : BlueprintComponent
{
	[field: SerializeField]
	public BpRef<BlueprintCase> Case { get; private set; }
}
