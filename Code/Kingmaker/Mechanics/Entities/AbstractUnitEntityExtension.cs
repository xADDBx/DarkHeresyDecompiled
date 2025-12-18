using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Mechanics.Entities;

public static class AbstractUnitEntityExtension
{
	[NotNull]
	public static AbstractUnitEntity ToAbstractUnitEntity(this IAbstractUnitEntity entity)
	{
		return (AbstractUnitEntity)entity;
	}

	[NotNull]
	public static IBaseUnitEntity ToIBaseUnitEntity(this IAbstractUnitEntity entity)
	{
		return (IBaseUnitEntity)entity;
	}

	[NotNull]
	public static BaseUnitEntity ToBaseUnitEntity(this IAbstractUnitEntity entity)
	{
		return (BaseUnitEntity)entity;
	}

	public static bool IsMovementLockedByGameModeOrCombat([NotNull] this AbstractUnitEntity entity)
	{
		GameModeType currentModeType = Game.Instance.CurrentModeType;
		if (currentModeType == GameModeType.Cutscene)
		{
			return true;
		}
		if (currentModeType == GameModeType.CutsceneGlobalMap)
		{
			return true;
		}
		if (currentModeType == GameModeType.Dialog)
		{
			return true;
		}
		if (currentModeType == GameModeType.GameOver)
		{
			return true;
		}
		if (entity.IsInCombat)
		{
			return true;
		}
		return false;
	}
}
