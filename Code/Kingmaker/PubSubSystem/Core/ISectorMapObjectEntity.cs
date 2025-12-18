using System;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

[Obsolete("DH")]
public interface ISectorMapObjectEntity : IMechanicEntity, IEntity, IDisposable
{
}
