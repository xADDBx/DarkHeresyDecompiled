using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Framework;

namespace Kingmaker.EntitySystem.Properties;

public interface IPropertyCalculatorComponent
{
	ContextPropertyName Name { get; }

	SaveToContextType SaveToContext { get; }

	int GetValue(IEvalContext context, MechanicEntity currentEntity);
}
