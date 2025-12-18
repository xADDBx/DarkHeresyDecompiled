using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.EntityInfo;

public class UnitPlaceholderInfoProvider : IEntityInfoProvider<GameObject>
{
	private static readonly IEntityInfo m_InfoPlaceholder = new EntityInfoPlaceholder();

	public bool TryGetEntityInfo(GameObject gameObject, out IEntityInfo entityInfo)
	{
		if (gameObject.TryGetComponent<UnitEntityView>(out var _))
		{
			entityInfo = m_InfoPlaceholder;
			return true;
		}
		entityInfo = null;
		return false;
	}
}
