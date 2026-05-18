using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.DialogSystem;

public class DialogData
{
	public BlueprintDialog Dialog;

	public UnitReference Initiator;

	public UnitReference Unit;

	public EntityRef<MapObjectEntity> MapObject;

	public string CustomSpeakerName;
}
