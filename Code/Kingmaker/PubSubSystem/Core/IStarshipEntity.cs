using System;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

[Obsolete("DH")]
public interface IStarshipEntity : IBaseUnitEntity, IAbstractUnitEntity, IMechanicEntity, IEntity, IDisposable
{
}
