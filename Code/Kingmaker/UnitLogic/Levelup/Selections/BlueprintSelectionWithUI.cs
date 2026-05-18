using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Localization;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Selections;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
[TypeId("07e13201532d48f48a0af95488837418")]
public abstract class BlueprintSelectionWithUI : BlueprintSelection
{
	[Header("UI")]
	public SelectionUIPhaseType PhaseType;

	public LocalizedString Title;

	public LocalizedString CallToAction;

	[SerializeField]
	private BlueprintEncyclopediaEntryReference m_DescriptionEntry;

	public BlueprintEncyclopediaEntry DescriptionEntry => m_DescriptionEntry;
}
