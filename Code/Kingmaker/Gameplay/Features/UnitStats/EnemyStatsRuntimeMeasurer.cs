using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.GuidUtility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.UnitStats;

public static class EnemyStatsRuntimeMeasurer
{
	public readonly struct SweepResult
	{
		public readonly Dictionary<(EnemyDifficultyOption tier, int cr), int> Hp;

		public readonly List<string> Failures;

		public SweepResult(Dictionary<(EnemyDifficultyOption, int), int> hp, List<string> failures)
		{
			Hp = hp;
			Failures = failures;
		}
	}

	public static bool IsAvailable
	{
		get
		{
			if (Application.isPlaying && Game.Instance != null && Game.Instance.LoadedAreaState?.MainState != null)
			{
				return Game.Instance.CurrentlyLoadedArea != null;
			}
			return false;
		}
	}

	public static SweepResult MeasureSweep(BlueprintUnit blueprint, IReadOnlyList<int> crs, IReadOnlyList<EnemyDifficultyOption> tiers)
	{
		Dictionary<(EnemyDifficultyOption, int), int> dictionary = new Dictionary<(EnemyDifficultyOption, int), int>();
		List<string> list = new List<string>();
		if (!IsAvailable)
		{
			list.Add("EnemyStatsRuntimeMeasurer requires PlayMode + a loaded area.");
			return new SweepResult(dictionary, list);
		}
		SceneEntitiesState mainState = Game.Instance.LoadedAreaState.MainState;
		RuntimeAreaSettings settings = Game.Instance.LoadedAreaState.Settings;
		EnemyDifficultyOption value = SettingsRoot.Difficulty.EnemyDurability.GetValue();
		int? cROverride = settings.CROverride;
		try
		{
			foreach (EnemyDifficultyOption tier in tiers)
			{
				try
				{
					SettingsRoot.Difficulty.EnemyDurability.SetValueAndConfirm(tier);
				}
				catch (Exception ex)
				{
					list.Add($"SetTier({tier}) threw: {ex.Message}");
					continue;
				}
				foreach (int cr in crs)
				{
					try
					{
						settings.CROverride = cr;
						EventBus.RaiseEvent(delegate(IAreaCRChangedHandler h)
						{
							h.HandleAreaCRChanged();
						});
						BaseUnitEntity baseUnitEntity = SpawnUnit(blueprint, mainState);
						try
						{
							dictionary[(tier, cr)] = baseUnitEntity.Actor.GetStat(StatType.MaxHitPoints, null, default(StatContext), "MeasureSweep").ModifiedValue;
						}
						finally
						{
							Game.Instance.Controllers.EntityDestroyer.Destroy(baseUnitEntity);
						}
					}
					catch (Exception ex2)
					{
						list.Add($"({tier}, cr={cr}) threw: {ex2.Message}");
					}
				}
			}
		}
		finally
		{
			try
			{
				SettingsRoot.Difficulty.EnemyDurability.SetValueAndConfirm(value);
			}
			catch
			{
			}
			try
			{
				settings.CROverride = cROverride;
				EventBus.RaiseEvent(delegate(IAreaCRChangedHandler h)
				{
					h.HandleAreaCRChanged();
				});
			}
			catch
			{
			}
		}
		return new SweepResult(dictionary, list);
	}

	private static BaseUnitEntity SpawnUnit(BlueprintUnit blueprint, SceneEntitiesState sceneState)
	{
		UnitEntity unitEntity = Entity.Initialize(new UnitEntity(Uuid.Instance.CreateString(), isInGame: true, blueprint));
		unitEntity.AttachToViewOnLoad(null);
		Game.Instance.Controllers.EntitySpawner.SpawnEntityImmediately(unitEntity, sceneState);
		unitEntity.Restore();
		return unitEntity;
	}
}
