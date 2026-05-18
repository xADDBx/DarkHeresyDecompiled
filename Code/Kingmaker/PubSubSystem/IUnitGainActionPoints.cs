using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitGainActionPoints : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleUnitGainActionPoints(int actionPoints, [CanBeNull] IEvalContext context);
}
public interface IUnitGainActionPoints<TTag> : IUnitGainActionPoints, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitGainActionPoints, TTag>
{
}
