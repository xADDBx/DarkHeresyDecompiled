using Kingmaker.ElementsSystem.ContextData;

namespace Code.GameCore.Editor.Blueprints.BlueprintUnitEditorChecker;

public class BlueprintUnitCheckerInEditorContextData : ContextData<BlueprintUnitCheckerInEditorContextData>
{
	public int UnitCR;

	public BlueprintUnitCheckerInEditorContextData Setup(int cr)
	{
		UnitCR = cr;
		return this;
	}

	protected override void Reset()
	{
		UnitCR = 0;
	}
}
