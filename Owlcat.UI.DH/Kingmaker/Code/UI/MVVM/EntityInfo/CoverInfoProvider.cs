using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Framework.Pathfinding;
using Kingmaker.View.Covers;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.EntityInfo;

public class CoverInfoProvider : IEntityInfoProvider<GameObject>
{
	private readonly UIStrings m_UIStrings;

	public CoverInfoProvider(UIStrings uiStrings)
	{
		m_UIStrings = uiStrings;
	}

	public bool TryGetEntityInfo(GameObject gameObject, out IEntityInfo entityInfo)
	{
		if (!TryGetCoverType(gameObject, out var coverType) || coverType == LosCalculations.CoverType.Obstacle)
		{
			entityInfo = null;
			return false;
		}
		if (!TryGetTexts(coverType, out var title, out var description))
		{
			entityInfo = null;
			return false;
		}
		entityInfo = new EntitySimpleInfo
		{
			WorldPosition = gameObject.transform.position,
			Title = title,
			Description = description
		};
		return true;
	}

	private bool TryGetCoverType(GameObject gameObject, out LosCalculations.CoverType coverType)
	{
		if (gameObject.TryGetComponent<GridObstacle>(out var component))
		{
			coverType = component.Type;
			return true;
		}
		if (gameObject.TryGetComponent<ObstacleMarker>(out var component2))
		{
			coverType = component2.Type;
			return true;
		}
		coverType = LosCalculations.CoverType.Obstacle;
		return false;
	}

	private bool TryGetTexts(LosCalculations.CoverType coverType, out string title, out string description)
	{
		switch (coverType)
		{
		case LosCalculations.CoverType.Cover:
			title = m_UIStrings.CoverInfoTexts.CoverInfoTitle;
			description = m_UIStrings.CoverInfoTexts.CoverDescription;
			return true;
		case LosCalculations.CoverType.LosBlocker:
			title = m_UIStrings.CoverInfoTexts.LosBlockerInfoTitle;
			description = m_UIStrings.CoverInfoTexts.LosBlockerDescription;
			return true;
		default:
			title = null;
			description = null;
			return false;
		}
	}
}
