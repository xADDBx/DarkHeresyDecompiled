using System;
using System.Collections.Generic;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI;

[Serializable]
[TypeId("1e087bf1f5c147d9b68a427e4d36e93e")]
public abstract class EntityListEvaluator : GenericEvaluator<List<Entity>>
{
}
