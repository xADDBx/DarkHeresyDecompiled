using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.DialogSystem;

public interface IDialogContext
{
	BlueprintDialog Dialog { get; }

	string CurrentSpeakerName { get; }

	BlueprintUnit CurrentSpeakerBlueprint { get; }

	BaseUnitEntity ActingUnit { get; }

	void Update(BlueprintCue currentCue);

	void Update(BlueprintAnswer answer, BaseUnitEntity manualUnitSelection);
}
