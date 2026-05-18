using Kingmaker.Code.Framework.VO;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework;
using Kingmaker.Gameplay.Features.Cohesion;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.Visual.Sound;

public class WithinPariahAuraAsksController : BaseAsksController, IEntityEnterCohesionRangeHandler, ISubscriber<IUnitEntity>, ISubscriber
{
	public void HandleEntityEnterCohesionRange(MechanicEntity entity)
	{
		if (!(ContextData<EventInvoker>.Current?.InvokerEntity is MechanicEntity mechanicEntity) || !mechanicEntity.Facts.Contains((BlueprintFeature?)VOSettings.Instance.PariahFeature) || (!entity.Facts.Contains((BlueprintFeature?)VOSettings.Instance.PsykanaUserFeature) && !entity.Facts.Contains((BlueprintFeature?)VOSettings.Instance.DeamonFeature)))
		{
			return;
		}
		using (EvalContext.PushAsksContext(mechanicEntity, entity))
		{
			entity.View.Asks?.WithinPariahAura.Schedule();
		}
	}
}
