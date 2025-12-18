using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.AI;

[ComponentName("AI/EntityHealthEvaluator")]
[TypeId("228f706eaf134c4b8d31a280b1ff29fc")]
public class EntityHealthEvaluator : IntEvaluator
{
	[SerializeReference]
	public EntityEvaluator Entity;

	public override string GetCaption()
	{
		return $"Health: {Entity}";
	}

	protected override int GetValueInternal()
	{
		int result = 0;
		if (Entity.TryGetValue(out var value))
		{
			PartHealth optional = value.GetOptional<PartHealth>();
			if (optional != null)
			{
				result = optional.HitPointsLeft;
			}
		}
		return result;
	}
}
