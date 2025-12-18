using System;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

[Obsolete("DH")]
public interface IStarSystemObjectEntity : IMapObjectEntity, IMechanicEntity, IEntity, IDisposable
{
}
