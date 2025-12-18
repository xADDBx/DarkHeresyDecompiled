using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.EntitySystem.Properties;

public interface IPropertyCalculatorComponent
{
	ContextPropertyName Name { get; }

	SaveToContextType SaveToContext { get; }

	int GetValue(MechanicsContext context, MechanicEntity currentEntity);

	int GetValue(PropertyContext context);
}
