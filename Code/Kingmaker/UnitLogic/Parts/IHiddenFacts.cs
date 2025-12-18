using System.Collections.Generic;
using Kingmaker.Blueprints.Facts;

namespace Kingmaker.UnitLogic.Parts;

public interface IHiddenFacts
{
	IEnumerable<BlueprintFact> Facts { get; }
}
