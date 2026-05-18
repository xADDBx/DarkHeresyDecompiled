using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.ContextContract;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Buffs.Components;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("188e834654fcf9a43af76ede9ba714e7")]
[ContextRole(ContextField.Caster, "original buff applier", Note = "if null, the buff self-removes")]
public class RemoveBuffIfCasterIsMissing : UnitBuffComponentDelegate, IAreaHandler, ISubscriber, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>
{
	public bool RemoveOnCasterDeath = true;

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		if (base.Context.MaybeCaster == null)
		{
			base.Buff.Remove();
		}
	}

	public void HandleUnitSpawned()
	{
	}

	public void HandleUnitDestroyed()
	{
		if (base.Context.MaybeCaster == null)
		{
			base.Buff.Remove();
		}
	}

	public void HandleUnitDeath()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (RemoveOnCasterDeath && base.Context.MaybeCaster == baseUnitEntity)
		{
			base.Buff.Remove();
		}
	}
}
