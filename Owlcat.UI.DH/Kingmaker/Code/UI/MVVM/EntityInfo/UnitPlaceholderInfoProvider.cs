using Kingmaker.View;

namespace Kingmaker.Code.UI.MVVM.EntityInfo;

public class UnitPlaceholderInfoProvider : IEntityInfoProvider<GameObjectInfo>
{
	private static readonly IEntityInfo m_InfoPlaceholder = new EntityInfoPlaceholder();

	public bool TryGetEntityInfo(GameObjectInfo info, out IEntityInfo entityInfo)
	{
		if (!info.GameObject || !info.IsHighlighted || !info.IsTurnBasedMode)
		{
			entityInfo = null;
			return false;
		}
		if (info.GameObject.TryGetComponent<UnitEntityView>(out var _))
		{
			entityInfo = m_InfoPlaceholder;
			return true;
		}
		entityInfo = null;
		return false;
	}
}
