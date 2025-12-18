using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("9989d1d3050345df9a958c098f1f2740")]
public sealed class BlueprintClueNote : BlueprintScriptableObject
{
	public LocalizedString Text;

	public ConditionsChecker Condition;

	public bool IsVisible => Condition.Check();
}
