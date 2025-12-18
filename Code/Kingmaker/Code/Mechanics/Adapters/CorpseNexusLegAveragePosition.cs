using System;
using System.Linq;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
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
		MechanicEntity mechanicEntity = SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current?.MaybeCaster;
		UnitPartCorpseNexusLegs unitPartCorpseNexusLegs = mechanicEntity?.GetOptional<UnitPartCorpseNexusLegs>();
		if (unitPartCorpseNexusLegs == null)
		{
			if (mechanicEntity == null)
			{
				throw new FailToEvaluateException(this);
			}
			return mechanicEntity.Position;
		}
		Vector3[] source = unitPartCorpseNexusLegs.Legs.Select((CorpseNexusLegData p) => p.Unit.Position).ToArray();
		if (source.Empty())
		{
			return mechanicEntity.Position;
		}
		return new Vector3(source.Average((Vector3 p) => p.x), source.Average((Vector3 p) => p.y), source.Average((Vector3 p) => p.z));
	}
}
