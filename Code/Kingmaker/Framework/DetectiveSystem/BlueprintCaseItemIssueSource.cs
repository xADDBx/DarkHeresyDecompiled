using System;
using Kingmaker.Blueprints;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
[TypeId("6dee747981784a2c96dd59ea542cc5db")]
public class BlueprintCaseItemIssueSource : BlueprintScriptableObject
{
	public CaseItemIssueType IssueType;

	public LocalizedString Description;
}
