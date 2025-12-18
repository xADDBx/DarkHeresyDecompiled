using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.CharacterSystem;
using Owlcat;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public static class UnitHologramExtension
{
	public static UnitHologram CreateHologram([NotNull] this BaseUnitEntity unit)
	{
		UnitHologram unitHologram = null;
		UnitEntityView unitEntityView = null;
		if (unit.View.CharacterAvatar == null && unit.View.GetComponentInChildren<StarshipView>() != null)
		{
			LogChannel.Default.Warning(unit.View, "This unit is not a Character nor a Starship, creating a hologram for this thing isn't possible.");
			return null;
		}
		Skeleton skeleton = unit.View.CharacterAvatar.Skeleton;
		HologramRaceFx[] hologramPrefabs = ConfigRoot.Instance.FxRoot.HologramPrefabs;
		Transform transform = null;
		HologramRaceFx[] array = hologramPrefabs;
		foreach (HologramRaceFx hologramRaceFx in array)
		{
			if (hologramRaceFx.Race == null)
			{
				LogChannel.Default.Error(unit.View, "No Skeletal config in HologramFx list in BlueprintFxRoot was found.");
			}
			else if (hologramRaceFx.HologramPrefab == null)
			{
				LogChannel.Default.Error(unit.View, "No HologramPrefab in HologramFx list in BlueprintFxRoot was found.");
			}
			else if (hologramRaceFx.Race == skeleton)
			{
				unitEntityView = SetupHologramPrefab(hologramRaceFx.HologramPrefab, unit);
				if (unitEntityView != null)
				{
					transform = unitEntityView.transform.FindChildRecursive("R_WeaponBone");
				}
				break;
			}
		}
		GameObject gameObject = null;
		if (unit.Body.PrimaryHand.MaybeWeapon?.Blueprint.VisualParameters.Model != null)
		{
			gameObject = UnityEngine.Object.Instantiate(unit.Body.PrimaryHand.Weapon.Blueprint.VisualParameters.Model);
		}
		if (unitEntityView == null && ConfigRoot.Instance.FxRoot.DefaultHologramPrefab != null)
		{
			unitEntityView = SetupHologramPrefab(ConfigRoot.Instance.FxRoot.DefaultHologramPrefab, unit);
		}
		if (unitEntityView == null)
		{
			LogChannel.Default.Error(unit.View, "Cannot create hologram for the unit. Check BlueprintFxRoot so appropriate data was set in HologramPrefabArray.");
			return null;
		}
		try
		{
			unitEntityView.gameObject.name = unit.View.gameObject.name + " (UnitHologram)";
			unitEntityView.GetComponentsInChildren<IKController>().ForEach(UnityEngine.Object.Destroy);
			unitEntityView.GetComponentsInChildren<UnitMovementAgent>().ForEach(UnityEngine.Object.Destroy);
			unitHologram = unitEntityView.gameObject.EnsureComponent<UnitHologram>();
			unitEntityView.CharacterAvatar = unitEntityView.GetComponent<Character>();
			if (gameObject != null && transform != null)
			{
				gameObject.transform.position = transform.position;
				gameObject.transform.parent = transform.transform;
				gameObject.transform.localRotation = Quaternion.identity;
				gameObject.transform.localScale = Vector3.one;
				CableController componentInChildren = gameObject.GetComponentInChildren<CableController>(includeInactive: true);
				if (componentInChildren != null)
				{
					componentInChildren.gameObject.SetActive(value: false);
				}
			}
			unitHologram.Setup(unitEntityView, unit.View);
		}
		catch (Exception ex)
		{
			PFLog.Entity.Exception(ex);
			UnityEngine.Object.Destroy(unitEntityView.gameObject);
		}
		return unitHologram;
	}

	[CanBeNull]
	private static UnitEntityView SetupHologramPrefab(PrefabLink holoPrefabLink, BaseUnitEntity unit)
	{
		UnitEntityView component = UnityEngine.Object.Instantiate(holoPrefabLink.Load()).GetComponent<UnitEntityView>();
		if (component == null)
		{
			LogChannel.Default.Error(unit.View, "No UnitEntityView in HologramPrefab. No Hologram will be created. Check BlueprintFxRoot.");
			return null;
		}
		return component;
	}
}
