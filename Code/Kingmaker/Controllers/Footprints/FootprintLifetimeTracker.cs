using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.Mechanics.Entities;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Controllers.Footprints;

internal class FootprintLifetimeTracker
{
	private static readonly int _BaseColorPropertyId = Shader.PropertyToID("_BaseColor");

	private readonly List<Footprint> _active = new List<Footprint>();

	private readonly List<Footprint> _fading = new List<Footprint>();

	public void Register(Footprint footprint)
	{
		_active.Add(footprint);
	}

	public void Tick(float dt, FxRoot fxRoot, Dictionary<AbstractUnitEntity, UnitFootprintState> unitStates, FootprintPool pool)
	{
		foreach (var (_, unitFootprintState2) in unitStates)
		{
			if (unitFootprintState2.ActiveFootprints == null)
			{
				continue;
			}
			Footprint footprint = ((unitFootprintState2.ActiveFootprints.Count > 0) ? unitFootprintState2.ActiveFootprints[0] : null);
			if (footprint == null)
			{
				continue;
			}
			int num = (unitFootprintState2.InParty ? fxRoot.MaxFootprintsCountPerPlayerUnit : Mathf.RoundToInt((float)fxRoot.MaxFootprintsCountPerPlayerUnit * fxRoot.MaxFootprintsCountModForNPC));
			if ((double)unitFootprintState2.ActiveFootprints.Count > (double)num * 0.75 || footprint.TimeLeft <= 0f)
			{
				footprint.TimeLeft = null;
				float num2 = ((unitFootprintState2.ActiveFootprints.Count > num) ? (fxRoot.FadeOutTimeSeconds * (1f - 1f / (float)(unitFootprintState2.ActiveFootprints.Count - num))) : 0f);
				footprint.FadeOutTime = ((!footprint.FadeOutTime.HasValue) ? num2 : Math.Max(num2, footprint.FadeOutTime.Value));
				footprint.MeshRenderer.material.renderQueue = 2000;
				if (!_fading.Contains(footprint))
				{
					_active.Remove(footprint);
					_fading.Add(footprint);
				}
			}
		}
		List<Footprint> value;
		using (CollectionPool<List<Footprint>, Footprint>.Get(out value))
		{
			foreach (Footprint item in _fading)
			{
				item.FadeOutTime += dt;
				float progress = item.FadeOutTime.Value / fxRoot.FadeOutTimeSeconds;
				UpdateFadeOut(item.MeshRenderer, progress);
				if (item.FadeOutTime >= fxRoot.FadeOutTimeSeconds)
				{
					value.Add(item);
				}
			}
			foreach (Footprint item2 in value)
			{
				_fading.Remove(item2);
				item2.UnitList.Remove(item2);
				pool.Return(item2);
			}
			foreach (Footprint item3 in _active)
			{
				float? timeLeft = item3.TimeLeft;
				if (timeLeft.HasValue && timeLeft.GetValueOrDefault() > 0f)
				{
					item3.TimeLeft -= dt;
				}
			}
		}
	}

	public void ReleaseAll(FootprintPool pool)
	{
		foreach (Footprint item in _active)
		{
			pool.Return(item);
		}
		foreach (Footprint item2 in _fading)
		{
			pool.Return(item2);
		}
		_active.Clear();
		_fading.Clear();
	}

	private static void UpdateFadeOut(MeshRenderer meshRenderer, float progress)
	{
		progress = Math.Max(0f, Math.Min(1f, progress));
		SetAlpha(meshRenderer, 1f - progress);
	}

	private static void SetAlpha(MeshRenderer meshRenderer, float alpha)
	{
		if ((bool)meshRenderer)
		{
			Color color = meshRenderer.material.GetColor(_BaseColorPropertyId);
			color.a = alpha;
			meshRenderer.material.SetColor(_BaseColorPropertyId, color);
		}
	}
}
