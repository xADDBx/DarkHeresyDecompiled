using Kingmaker.Blueprints;

namespace Kingmaker.ElementsSystem.Interfaces;

public interface IInitializableElement
{
	void InitInEditor(string propertyPath, SimpleBlueprint ownerBlueprint);
}
