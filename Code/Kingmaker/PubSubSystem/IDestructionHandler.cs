using System;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;

namespace Kingmaker.PubSubSystem;

[Obsolete]
public interface IDestructionHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleDestructionSuccess(MapObjectView mapObjectView);

	void HandleDestructionFail(MapObjectView mapObjectView);
}
