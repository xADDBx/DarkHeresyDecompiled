using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Cohesion;

[Serializable]
[TypeId("947e7eeba8f227d4b923544882d425fc")]
public sealed class CohesionGetter : IntPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Cohesion range";
	}

	protected override int GetBaseValue()
	{
		return base.CurrentEntity.GetOptional<PartCohesion>()?.Range ?? 0;
	}
}
