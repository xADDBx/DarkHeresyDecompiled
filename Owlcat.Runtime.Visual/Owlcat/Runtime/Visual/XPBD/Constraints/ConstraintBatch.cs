using System;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Constraints;

[Serializable]
public struct ConstraintBatch
{
	public ConstraintType Type;

	public int2 OffsetCount;
}
