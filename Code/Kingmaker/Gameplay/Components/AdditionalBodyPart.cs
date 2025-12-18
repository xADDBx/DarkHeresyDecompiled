using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ComponentName("CriticalEffects/AdditionalBodyPart")]
[TypeId("e2cb9766a786434a81fd5f5dc1c1fbdb")]
public sealed class AdditionalBodyPart : UnitFactComponentDelegate
{
	[ValidateNotNull]
	public BpRef<BlueprintBodyPart> BodyPart;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartAdditionalBodyParts>().Add(BodyPart, base.Fact, this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartAdditionalBodyParts>()?.Remove(base.Fact, this);
	}
}
