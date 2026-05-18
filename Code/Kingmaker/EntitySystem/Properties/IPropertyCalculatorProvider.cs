using System.Collections.Generic;

namespace Kingmaker.EntitySystem.Properties;

public interface IPropertyCalculatorProvider
{
	IReadOnlyList<IPropertyCalculatorComponent> GetPropertyCalculators();
}
