using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("4762961218904cae98aa6d938030a09f")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public sealed class BlueprintCase : BlueprintScriptableObject
{
	[Serializable]
	[Obsolete("New Question/Answer approach, WIP")]
	public sealed class ConclusionsPack
	{
		public ConditionsChecker Conditions = new ConditionsChecker();

		public bool Fake;

		[ValidateNoNullEntries]
		public BpRef<BlueprintConclusionType>[] Questions = new BpRef<BlueprintConclusionType>[0];

		[ValidateNoNullEntries]
		public BpRef<BlueprintConclusion>[] Conclusions = new BpRef<BlueprintConclusion>[0];
	}

	public LocalizedString Name = new LocalizedString();

	public LocalizedString Description = new LocalizedString();

	public Sprite Icon;

	[ValidateNoNullEntries]
	public BpRef<BlueprintClue>[] Clues = new BpRef<BlueprintClue>[0];

	[ValidateNoNullEntries]
	public BpRef<BlueprintConclusion>[] Conclusions = new BpRef<BlueprintConclusion>[0];

	[InfoBox("Первый вопрос из этого списка доступный по кондишену считает актуальным, игрок видит его в детективном журнале.")]
	[ValidateNotEmpty]
	[ValidateNoNullEntries]
	public BpRef<BlueprintCaseQuestion>[] Questions = new BpRef<BlueprintCaseQuestion>[0];

	public ActionList OnOpen = new ActionList();

	public ActionList OnClose = new ActionList();

	[Obsolete("New Question/Answer approach, WIP")]
	[SerializeField]
	private ConclusionsPack[] m_CorrectConclusions = new ConclusionsPack[0];

	[Obsolete("New Question/Answer approach, WIP")]
	public IEnumerable<BlueprintConclusion> CorrectConclusions => m_CorrectConclusions.FirstItem((ConclusionsPack i) => i.Conditions.Check())?.Conclusions.Dereference() ?? Enumerable.Empty<BlueprintConclusion>();

	[Obsolete("New Question/Answer approach, WIP")]
	public IEnumerable<BlueprintConclusionType> ObsoleteQuestions => m_CorrectConclusions.FirstItem((ConclusionsPack i) => i.Conditions.Check())?.Questions.Dereference() ?? Enumerable.Empty<BlueprintConclusionType>();

	public IEnumerable<BlueprintCaseItem> AllItems => Clues.Dereference().OfType<BlueprintCaseItem>().Concat(Clues.Dereference().SelectMany((BlueprintClue i) => i.Addendums.Dereference()))
		.Concat(Conclusions.Dereference());
}
