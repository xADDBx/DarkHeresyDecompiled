using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.CharacterSystem;
using UnityEngine;

namespace Kingmaker.Controllers.Footprints;

internal static class FootprintPrefabResolver
{
	private static GameObject _CachedStubFootprint;

	private static readonly GameObject[] _DefaultFootprintsByRace = new GameObject[EnumUtils.GetMaxValuePlusOne<CharacterStudio.Race>()];

	internal static GameObject CachedStub => _CachedStubFootprint;

	[CanBeNull]
	public static GameObject Resolve(AbstractUnitEntity unit, int footIndex)
	{
		PrefabLink[] footprintsOverride = unit.Blueprint.VisualSettings.FootprintsOverride;
		if (footprintsOverride.Length != 0)
		{
			if (footIndex >= footprintsOverride.Length)
			{
				PFLog.TechArt.Error("FootprintsController: index in PlaceFootprint event from " + unit.AnimationManager.CurrentAction.ActiveAnimation?.GetActiveClip()?.name + " animation is out of range for FootprintsOverride array in blueprint: " + unit.Blueprint.name);
			}
			else
			{
				GameObject gameObject = footprintsOverride[footIndex].Load();
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		AbstractUnitEntityView abstractUnitEntityView = unit.View.AsAbstractUnitEntityView();
		if (abstractUnitEntityView.Footprints.Length == 0)
		{
			return GetDefaultFootprintPrefab(abstractUnitEntityView);
		}
		if (footIndex >= abstractUnitEntityView.Footprints.Length)
		{
			PFLog.TechArt.Error("FootprintsController: index in PlaceFootprint event from " + unit.AnimationManager.CurrentAction.ActiveAnimation?.GetActiveClip()?.name + " animation is out of range for Footprints array in prefab: " + unit.View.name);
			return null;
		}
		return abstractUnitEntityView.Footprints[footIndex];
	}

	[CanBeNull]
	private static GameObject GetDefaultFootprintPrefab(AbstractUnitEntityView view)
	{
		if (view.CharacterAvatar == null)
		{
			PFLog.TechArt.Warning("Footprints Controller: Unit " + view.name + " does not have an overridden footprint prefab on any of the levels, so a stub was used");
			if ((object)_CachedStubFootprint == null)
			{
				_CachedStubFootprint = ConfigRoot.Instance.FxRoot.StubFootprint.Load();
			}
			return _CachedStubFootprint;
		}
		CharacterStudio.Race item = view.CharacterAvatar.GenderAndRace.race;
		ref GameObject reference = ref _DefaultFootprintsByRace[(int)item];
		if (reference != null)
		{
			return reference;
		}
		FxRoot fxRoot = ConfigRoot.Instance.FxRoot;
		reference = item switch
		{
			CharacterStudio.Race.Human => fxRoot.DefaultHumanFootprint.Load(), 
			CharacterStudio.Race.Eldar => fxRoot.DefaultEldarFootprint.Load(), 
			CharacterStudio.Race.Spacemarine => fxRoot.DefaultSpaceMarineFootprint.Load(), 
			CharacterStudio.Race.Kroot => fxRoot.DefaultKrootFootprint.Load(), 
			CharacterStudio.Race.Ogryn => fxRoot.DefaultOgrynFootprint.Load(), 
			_ => null, 
		};
		return reference;
	}
}
