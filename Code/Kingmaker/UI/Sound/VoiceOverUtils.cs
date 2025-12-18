using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.UI.Sound;

public static class VoiceOverUtils
{
	public static bool TryGetVoGuid(this Entity entity, out string voGuid)
	{
		voGuid = string.Empty;
		if (entity is AbstractUnitEntity abstractUnitEntity)
		{
			voGuid = abstractUnitEntity.VoGuid;
			return true;
		}
		if (entity is MapObjectEntity mapObjectEntity && mapObjectEntity.View.NeedsVoiceOver)
		{
			voGuid = mapObjectEntity.View.VoId.Guid;
			return true;
		}
		return false;
	}
}
