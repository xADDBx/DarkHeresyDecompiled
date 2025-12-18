using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Interaction;

public interface IInteractionRestriction
{
	bool CheckRestriction(BaseUnitEntity user);

	void ShowSuccessBark(BaseUnitEntity user);

	void ShowRestrictionBark(BaseUnitEntity user);
}
