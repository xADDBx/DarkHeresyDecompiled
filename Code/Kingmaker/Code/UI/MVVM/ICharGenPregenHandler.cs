using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface ICharGenPregenHandler : ISubscriber
{
	void HandleSetPregen([CanBeNull] BaseUnitEntity unit);
}
