using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Controllers.Footprints;

internal class FootprintSpawner
{
	private static readonly int _BaseColorPropertyId = Shader.PropertyToID("_BaseColor");

	private static readonly int _BaseAlphaScalePropertyId = Shader.PropertyToID("_AlphaScale");

	private static readonly int _BaseBumpScalePropertyId = Shader.PropertyToID("_BumpScale");

	private static readonly int _BaseRoughnessPropertyId = Shader.PropertyToID("_Roughness");

	private readonly FootprintPool _pool;

	private readonly FootprintLifetimeTracker _tracker;

	public FootprintSpawner(FootprintPool pool, FootprintLifetimeTracker tracker)
	{
		_pool = pool;
		_tracker = tracker;
	}

	public bool TrySpawn(AbstractUnitEntity unit, UnitFootprintState state, FxRoot fxRoot)
	{
		if (unit.View == null)
		{
			PFLog.TechArt.Warning("FootprintsController: unit " + unit.Blueprint.name + " has no View, skipping footprint");
			return false;
		}
		if (Vector3.Distance(state.PreviousFootPosition, state.PendingFoot.Value.Transform.position) < fxRoot.MinDistanceBetweenFootprints)
		{
			return false;
		}
		state.PreviousFootPosition = new Vector3(state.PendingFoot.Value.Transform.position.x, state.PendingFoot.Value.Transform.position.y, state.PendingFoot.Value.Transform.position.z);
		SurfaceType? groundType = SoundSurfaceMap.GetSurfaceSoundTypeSwitch(state.PendingFoot.Value.Transform.position);
		if (!groundType.HasValue)
		{
			if (state.InParty)
			{
				PFLog.Audio.Error("Can not create footprints, Surface Type Object returns null. Check sound surfaces setup.");
			}
			return false;
		}
		GameObject gameObject = FootprintPrefabResolver.Resolve(unit, state.PendingFootIndex);
		if (gameObject == null)
		{
			return false;
		}
		GameObject gameObject2 = FootprintPrefabResolver.CachedStub ?? ConfigRoot.Instance.FxRoot.StubFootprint.Load();
		bool flag = gameObject == gameObject2;
		FxRoot.FootprintSurfaceSettings result = null;
		if (!flag && !fxRoot.FootprintsSettings.TryFind((FxRoot.FootprintSurfaceSettings x) => x.GroundType == groundType, out result))
		{
			PFLog.TechArt.Warning($"FootprintsController: no FootprintSurfaceSettings found for ground type {groundType} on unit {unit.Blueprint.name}");
			return false;
		}
		MeshRenderer componentNonAlloc = gameObject.GetComponentNonAlloc<MeshRenderer>();
		if (componentNonAlloc == null)
		{
			PFLog.TechArt.Error("FootprintsController: footprint prefab " + gameObject.name + " has no MeshRenderer component");
			return false;
		}
		Footprint footprint = _pool.Rent(gameObject, state.PendingFoot.Value.LeftSided);
		state.ActiveFootprints.Add(footprint);
		footprint.UnitList = state.ActiveFootprints;
		footprint.MeshRenderer.material = componentNonAlloc.sharedMaterial;
		footprint.MeshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
		if (flag)
		{
			footprint.MeshRenderer.material.SetColor(_BaseColorPropertyId, Color.magenta);
		}
		else
		{
			footprint.MeshRenderer.material.SetFloat(_BaseAlphaScalePropertyId, result.FootprintAlphaScale);
			footprint.MeshRenderer.material.SetFloat(_BaseBumpScalePropertyId, result.FootprintBumpScale);
			footprint.MeshRenderer.material.SetFloat(_BaseRoughnessPropertyId, result.FootprintRoughness);
			Color footprintTintColor = result.FootprintTintColor;
			footprintTintColor.a = 1f;
			footprint.MeshRenderer.material.SetColor(_BaseColorPropertyId, footprintTintColor);
		}
		footprint.Transform.position = state.PendingFoot.Value.Transform.position;
		footprint.Transform.rotation = state.PendingFoot.Value.Transform.rotation;
		footprint.TimeLeft = fxRoot.DefaultLifetimeSeconds;
		footprint.GameObject.name = "footprint_" + unit.View.name + "_" + groundType.ToString();
		footprint.GameObject.SetActive(value: true);
		_tracker.Register(footprint);
		return true;
	}
}
