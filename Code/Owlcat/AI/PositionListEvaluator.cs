using System;
using System.Collections.Generic;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.AI;

[Serializable]
[TypeId("01f53521ff214feb96baa4cf8f0d534a")]
public abstract class PositionListEvaluator : GenericEvaluator<List<Vector3>>
{
}
