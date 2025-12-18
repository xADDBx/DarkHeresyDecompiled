using System;
using System.Collections.Generic;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Owlcat.AI;

[Serializable]
[TypeId("c7243aa9c0f8412099389c99dd81bc5a")]
public abstract class GraphNodeListEvaluator : GenericEvaluator<List<GraphNode>>
{
}
