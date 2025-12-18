using System.Collections.Generic;

namespace Owlcat.EntityBlackboard;

public interface IRuntimeEntityBlackboard
{
	IEnumerable<RuntimeVariable> Variables { get; }
}
