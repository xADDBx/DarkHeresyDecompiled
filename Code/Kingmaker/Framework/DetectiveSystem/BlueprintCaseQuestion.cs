using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
[TypeId("642789f7a249488e91cede47d1b1ba0c")]
public sealed class BlueprintCaseQuestion : BlueprintScriptableObject
{
	public LocalizedString Name = new LocalizedString();

	public LocalizedString Description = new LocalizedString();

	public ConditionsChecker Condition = new ConditionsChecker();

	public bool NoAnswer;

	[SerializeField]
	[HideIf("NoAnswer")]
	public BpRef<BlueprintCaseAnswer> RightAnswer = new BpRef<BlueprintCaseAnswer>();

	[ValidateNoNullEntries]
	[SerializeField]
	[HideIf("NoAnswer")]
	public BpRef<BlueprintCaseAnswer>[] WrongAnswers = new BpRef<BlueprintCaseAnswer>[0];

	public IEnumerable<BpRef<BlueprintCaseAnswer>> AllAnswers => WrongAnswers.Prepend<BpRef<BlueprintCaseAnswer>>(RightAnswer);
}
