using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Visual.Sound;

public class ItemInteractionsAsksController : BaseAsksController, IInsertItemFailHandler, ISubscriber<IItemEntity>, ISubscriber
{
	void IInsertItemFailHandler.HandleInsertFail(MechanicEntity owner)
	{
		using (EvalContext.PushAsksContext(owner, owner))
		{
			owner?.View.Asks?.CantDo.Schedule(is2D: true);
		}
	}
}
