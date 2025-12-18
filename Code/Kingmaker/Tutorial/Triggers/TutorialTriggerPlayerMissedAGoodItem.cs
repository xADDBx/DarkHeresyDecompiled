using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[TypeId("8cb1bfc127694b5e97d2b0862d73f6cf")]
public class TutorialTriggerPlayerMissedAGoodItem : BlueprintComponent
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintItemReference[] m_ItemsReferences;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintAreaReference m_AreaReferences;

	private BlueprintItem[] Items => m_ItemsReferences.Select((BlueprintItemReference i) => i.Get()).ToArray();
}
