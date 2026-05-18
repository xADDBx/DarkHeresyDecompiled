using System;
using System.Linq;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Mechanics.Adapters;

[Serializable]
[TypeId("b1b7442c09a34685be00c3f2190cd755")]
public class CorpseNexusLegAveragePosition : PositionEvaluator
{
	public override string GetCaption()
	{
		return "Evaluate target position of Context";
	}

	protected override Vector3 GetValueInternal()
	{
		MechanicEntity caster = EvalContext.Current.Caster;
		UnitPartCorpseNexusLegs unitPartCorpseNexusLegs = caster?.GetOptional<UnitPartCorpseNexusLegs>();
		if (unitPartCorpseNexusLegs == null)
		{
			if (caster == null)
			{
				throw new FailToEvaluateException(this);
			}
			return caster.Position;
		}
		Vector3[] source = unitPartCorpseNexusLegs.Legs.Select((CorpseNexusLegData p) => p.Unit.Position).ToArray();
		if (source.Empty())
		{
			return caster.Position;
		}
		return new Vector3(source.Average((Vector3 p) => p.x), source.Average((Vector3 p) => p.y), source.Average((Vector3 p) => p.z));
	}
}
