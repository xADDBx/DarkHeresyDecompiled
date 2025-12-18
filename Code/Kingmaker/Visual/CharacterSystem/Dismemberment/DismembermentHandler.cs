using System.Collections.Generic;
using System.Linq;
using Kingmaker._TmpTechArt;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment;

public static class DismembermentHandler
{
	public enum DeathType
	{
		Dismemberment,
		RagDoll,
		Animation
	}

	public static bool ShouldDismember(AbstractUnitEntity unit)
	{
		int num;
		if (!unit.Blueprint.VisualSettings.IsNotUseDismember && !unit.Features.SuppressedDismember && !(unit is BaseUnitEntity { IsMainCharacter: not false }))
		{
			UnitPartCompanion optional = unit.GetOptional<UnitPartCompanion>();
			if (optional == null || optional.State == CompanionState.None)
			{
				num = ((unit.OriginalSize >= Size.Huge) ? 1 : 0);
				goto IL_0057;
			}
		}
		num = 1;
		goto IL_0057;
		IL_0057:
		bool isFinallyDead = unit.LifeState.IsFinallyDead;
		if (num != 0 || !isFinallyDead)
		{
			return false;
		}
		if (unit.LifeState.ForceDismember == UnitDismemberType.ForcedNone)
		{
			return false;
		}
		if (unit.LifeState.ForceDismember != 0)
		{
			return true;
		}
		return GetDeathType(unit) == DeathType.Dismemberment;
	}

	public static UnitDismemberType GetDismemberType(AbstractUnitEntity unit)
	{
		UnitDismemberType forceDismember = unit.LifeState.ForceDismember;
		if (forceDismember != 0 && forceDismember != UnitDismemberType.ForcedNone)
		{
			return forceDismember;
		}
		bool flag = false;
		bool flag2 = false;
		BlueprintComponent[] array = unit?.Health.LastHandledDamage?.Reason.Context?.SourceAbilityBlueprint?.ComponentsArray;
		if (array != null)
		{
			if (array.Any((BlueprintComponent comp) => comp is InPowerDismemberComponent))
			{
				flag = true;
			}
			if (array.Any((BlueprintComponent comp) => comp is SplitDismemberComponent))
			{
				flag2 = true;
			}
		}
		bool flag3 = CanDismemberLimbsApart(unit);
		flag2 = flag2 && flag3;
		HitSystemRoot hitSystemRoot = ConfigRoot.Instance.HitSystemRoot;
		if (flag && flag2)
		{
			float num = PFStatefulRandom.View.Range(0f, 100f);
			if (hitSystemRoot.LimbsApartDismembermentChance > 0f && num <= hitSystemRoot.LimbsApartDismembermentChance)
			{
				return UnitDismemberType.LimbsApart;
			}
			return UnitDismemberType.InPower;
		}
		if (flag)
		{
			return UnitDismemberType.InPower;
		}
		if (flag2)
		{
			return UnitDismemberType.LimbsApart;
		}
		if (flag3)
		{
			float num2 = PFStatefulRandom.View.Range(0f, 100f);
			if (hitSystemRoot.LimbsApartDismembermentChance > 0f && num2 <= hitSystemRoot.LimbsApartDismembermentChance)
			{
				return UnitDismemberType.LimbsApart;
			}
			return UnitDismemberType.Normal;
		}
		return UnitDismemberType.Normal;
	}

	private static bool CanDismemberLimbsApart(AbstractUnitEntity unit)
	{
		if (unit.View.LimbsApartDismembermentRestricted)
		{
			return false;
		}
		if (unit.View.DismembermentManager == null)
		{
			return false;
		}
		if (unit.View.RigidbodyController == null)
		{
			return false;
		}
		if (unit.View.DismembermentManager.LastImpulse == null || Game.Instance.Controllers.TimeController.RealTime - unit.View.DismembermentManager.LastImpulse.Time > 0.2f.Seconds())
		{
			return false;
		}
		return unit.View.DismembermentManager.LastImpulse.DamageType != DamageType.Toxic;
	}

	public static DeathType GetDeathType(AbstractUnitEntity unit)
	{
		if (!unit.LifeState.IsFinallyDead)
		{
			return DeathType.Animation;
		}
		AbstractUnitEntityView view = unit.View;
		if (unit.LifeState.ForceDismember == UnitDismemberType.LimbsApart && view.DismembermentManager != null)
		{
			return DeathType.Dismemberment;
		}
		int num = 0;
		RuleDealDamage obj = unit.GetOptional<PartHealth>()?.LastHandledDamage;
		List<WeaponsListDism.PrefabInPair> weaponsChancesArray = ConfigRoot.Instance.BlueprintDismembermentRoot.WeaponsListDismArray.WeaponsChancesArray;
		GameObject gameObject = obj?.Reason.Ability?.Weapon?.Blueprint?.VisualParameters?.Model;
		string text = ((gameObject != null) ? gameObject.name : null);
		foreach (WeaponsListDism.PrefabInPair item in weaponsChancesArray)
		{
			if (text != null && item.PrefabPairGo != null && item.PrefabPairGo.name.ToLower().Equals(text.ToLower()))
			{
				num = item.PrefabPairInt;
			}
		}
		DeathType deathType = ((((unit.Random.Range(0, 100) < num) ? 1 : 0) != 1) ? DeathType.RagDoll : DeathType.Dismemberment);
		bool? obj2 = ConfigRoot.Instance.BlueprintDismembermentRoot?.WeaponsListDismArray.ForceRagdoll;
		bool? flag = ConfigRoot.Instance.BlueprintDismembermentRoot?.WeaponsListDismArray.ForceDismemberment;
		if (obj2 == true)
		{
			deathType = DeathType.RagDoll;
		}
		if (flag == true)
		{
			deathType = DeathType.Dismemberment;
		}
		if (!SettingsRoot.Game.Main.DismemberCharacters.GetValue())
		{
			deathType = DeathType.RagDoll;
		}
		if (deathType == DeathType.Dismemberment && unit.View.DismembermentManager == null)
		{
			deathType = DeathType.RagDoll;
		}
		if (deathType == DeathType.RagDoll && unit.View.RigidbodyController == null)
		{
			deathType = DeathType.Animation;
		}
		return deathType;
	}

	public static bool CanUseAnimation(AbstractUnitEntity unit)
	{
		return GetDeathType(unit) == DeathType.Animation;
	}

	public static void UseWithoutAnimationDeath(AbstractUnitEntity unit)
	{
		DeathType deathType = GetDeathType(unit);
		if (deathType != 0 && deathType != DeathType.RagDoll)
		{
			return;
		}
		AbstractUnitEntityView view = unit.View;
		if (!(view == null))
		{
			if (deathType == DeathType.Dismemberment)
			{
				ObjectExtensions.Or(view.DismembermentManager, null)?.StartDismemberment(unit.Random, unit.LifeState.DismembermentLimbsApartType);
			}
			if (deathType == DeathType.RagDoll)
			{
				ObjectExtensions.Or(view.RigidbodyController, null)?.StartRagdoll();
			}
			SpawnFxsOnDeath(unit, deathType);
		}
	}

	private static void SpawnFxsOnDeath(AbstractUnitEntity unit, DeathType type)
	{
		SpawnFxOnStart component = unit.View.GetComponent<SpawnFxOnStart>();
		if ((bool)component)
		{
			if (type == DeathType.Dismemberment)
			{
				component.HandleUnitDismemberment();
			}
			else
			{
				component.HandleUnitDeathRagdoll();
			}
		}
		BlueprintDismembermentRoot blueprintDismembermentRoot = ConfigRoot.Instance.BlueprintDismembermentRoot;
		BlueprintDismembermentRoot.FXDamagePair[] array = blueprintDismembermentRoot?.FXDamagePairs;
		if (array == null)
		{
			return;
		}
		BlueprintDismembermentRoot.PrefabDamagePair[] prefabDamagePairs = blueprintDismembermentRoot.PrefabDamagePairs;
		if (prefabDamagePairs == null)
		{
			return;
		}
		RuleDealDamage ruleDealDamage = unit.GetOptional<PartHealth>()?.LastHandledDamage;
		if (ruleDealDamage?.ResultDamage == null)
		{
			return;
		}
		if (ruleDealDamage.Reason.Ability?.FXSettings?.VisualFXSettings == null)
		{
			AbilityData ability = ruleDealDamage.Reason.Ability;
			if ((object)ability != null)
			{
				PFLog.TechArt.Error((ability.FXSettings != null) ? $"Ability {ability.Blueprint.name}: {ruleDealDamage.Reason.Ability.FXSettings} has no VisualFXSettings" : ("Ability " + ability.Blueprint.name + " has no FXSettings"));
			}
			return;
		}
		DamageType damageType = ruleDealDamage.ResultDamage.Type;
		ProjectileLink damageProjectileView = ruleDealDamage.Reason.Ability.FXSettings.VisualFXSettings.Projectiles.FirstOrDefault()?.View;
		PrefabLink prefabLink = prefabDamagePairs.FirstOrDefault((BlueprintDismembermentRoot.PrefabDamagePair i) => i.FxPrefab == damageProjectileView)?.FX2 ?? array.FirstOrDefault((BlueprintDismembermentRoot.FXDamagePair i) => i.DamageType == damageType)?.FX;
		if (!(prefabLink == null))
		{
			GameObject prefab = prefabLink.Load();
			if (type == DeathType.RagDoll)
			{
				FxHelper.SpawnFxOnEntity(prefab, unit.View);
			}
			if (type == DeathType.Dismemberment && unit.View.DismembermentManager != null)
			{
				FxHelper.SpawnFxOnGameObject(prefab, unit.View.gameObject);
			}
		}
	}
}
