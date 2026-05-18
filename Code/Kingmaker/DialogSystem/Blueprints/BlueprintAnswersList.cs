using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.DialogSystem.Blueprints;

[TypeId("52ca6275d0a87bf428811a8c24d89de4")]
public class BlueprintAnswersList : BlueprintAnswerBase
{
	public bool ShowOnce;

	[NotNull]
	public List<BlueprintAnswerBaseReference> Answers = new List<BlueprintAnswerBaseReference>();

	[Header("Requirements")]
	public AlignmentRequirement AlignmentRequirement = new AlignmentRequirement();

	[NotNull]
	public ConditionsChecker Conditions = new ConditionsChecker();

	public bool CanSelect()
	{
		if (ShowOnce && Game.Instance.DialogState.ShownAnswerListsContains(this))
		{
			DialogDebug.Add(this, "(show once) was selected before", Color.red);
			return false;
		}
		if (!Conditions.Check(this))
		{
			DialogDebug.Add(this, "conditions failed", Color.red);
			return false;
		}
		DialogDebug.Add(this, "answer list selected", Color.green);
		return true;
	}
}
