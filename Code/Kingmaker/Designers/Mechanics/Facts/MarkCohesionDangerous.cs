using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.Cohesion;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[ComponentName("AI/MarkCohesionDangerous")]
[AllowMultipleComponents]
[TypeId("7e7296ed20c74d9085749adef1097bc5")]
public class MarkCohesionDangerous : EntityFactComponentDelegate<BaseUnitEntity>
{
	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Parts.GetOptional<PartCohesion>()?.MarkAreaDangerous();
		base.OnActivate();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Parts.GetOptional<PartCohesion>()?.RemoveDangerousMark();
		base.OnDeactivate();
	}
}
