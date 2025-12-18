using System;
using Kingmaker.Blueprints;
using Kingmaker.Localization;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using OwlPack.Runtime;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
[TypeId("8a1ac1d88ef746d5a9d689d16f55681a")]
public sealed class BlueprintCaseAnswer : BlueprintScriptableObject
{
	[Serializable]
	public sealed class DegreeProgressionEntry
	{
		public LocalizedString Description = new LocalizedString();

		[ValidateNoNullEntries]
		public BpRef<BlueprintCaseItem>[] Items = new BpRef<BlueprintCaseItem>[0];
	}

	public LocalizedString Description = new LocalizedString();

	[InfoBox("BlueprintCaseItem, с которой связан ответ. Ответ становится доступным только при наличии этого айтема и любого айтема из DegreeProgression")]
	[ValidateNotNull]
	public BpRef<BlueprintCaseItem> RelatedItem = new BpRef<BlueprintCaseItem>();

	[InfoBox("Первый элемент в списке делает ответ доступным для выбора, каждый последующий элемент повышает степень обвинения. В каждом элементе можно указать список BlueprintCaseItem, для открытия степени обвинения любой из них должен быть у игрока.")]
	[ValidateNotEmpty]
	public DegreeProgressionEntry[] DegreeProgression = new DegreeProgressionEntry[0];
}
