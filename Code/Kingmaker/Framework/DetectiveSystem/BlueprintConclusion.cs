using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("6dfcbb1b09e64753b61577d95206012b")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public sealed class BlueprintConclusion : BlueprintCaseItem
{
	[Serializable]
	public class Source
	{
		[ValidateNotNull]
		public BpRef<BlueprintCaseItem> Item1;

		[ValidateNotNull]
		public BpRef<BlueprintCaseItem> Item2;

		public bool Contains(BlueprintCaseItem item)
		{
			if (!(Item1 == item))
			{
				return Item2 == item;
			}
			return true;
		}
	}

	[Obsolete("New Question/Answer approach, WIP")]
	[ValidateNotNull]
	public BpRef<BlueprintConclusionType> Type;

	public Source[] Sources = new Source[0];

	[ValidateNoNullEntries]
	public BpRef<BlueprintCaseItem>[] Refutations = new BpRef<BlueprintCaseItem>[0];

	public ConditionsChecker UnlockCondition = new ConditionsChecker();

	public bool ContainsSource(BlueprintCaseItem source1, BlueprintCaseItem source2)
	{
		return Sources.HasItem((Source i) => i.Contains(source1) && i.Contains(source2));
	}

	public bool ContainsSource(BlueprintCaseItem source)
	{
		return Sources.HasItem((Source i) => i.Contains(source));
	}
}
