using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Weakpoints;

public sealed class WeakpointsVisualizer : MonoBehaviour, IWeakpointAdded, ISubscriber<IBaseUnitEntity>, ISubscriber, IWeakpointRemoved, IViewAttachedHandler, ISubscriber<IEntity>
{
	private class Entry
	{
		public readonly Dictionary<WeakpointSide, GameObject> Marks = new Dictionary<WeakpointSide, GameObject>();
	}

	public GameObject WeakpointPrefab;

	private readonly Dictionary<UnitEntityView, Entry> _entries = new Dictionary<UnitEntityView, Entry>();

	private void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
	}

	private void UpdateWeakpoints(UnitEntityView view)
	{
		PartWeakpoints optional = view.Data.GetOptional<PartWeakpoints>();
		foreach (WeakpointSide value in EnumUtils.GetValues<WeakpointSide>())
		{
			Entry entry = _entries.Get(view);
			bool flag = entry != null && (bool)entry.Marks.Get(value);
			bool flag2 = optional?.HasWeakpoint(value) ?? false;
			if (flag && !flag2)
			{
				RemoveWeakpoint(view, value);
			}
			else if (!flag && flag2)
			{
				AddWeakpoint(view, value);
			}
		}
	}

	private void AddWeakpoint(UnitEntityView view, WeakpointSide side)
	{
		Entry entry2 = _entries.Get(view) ?? (_entries[view] = new Entry());
		if (!entry2.Marks.Get(side))
		{
			GameObject gameObject2 = (entry2.Marks[side] = UnityEngine.Object.Instantiate(GetWeakpointMarkPrefab(side), view.transform, worldPositionStays: true));
			GameObject gameObject3 = gameObject2;
			gameObject3.transform.position = view.transform.position;
			gameObject3.transform.rotation = view.transform.rotation;
			switch (side)
			{
			case WeakpointSide.Front:
				gameObject3.transform.position += view.transform.forward * view.Corpulence;
				break;
			case WeakpointSide.Left:
				gameObject3.transform.rotation = Quaternion.LookRotation(-view.transform.right, view.transform.up);
				gameObject3.transform.position -= view.transform.right * view.Corpulence;
				break;
			case WeakpointSide.Right:
				gameObject3.transform.rotation = Quaternion.LookRotation(view.transform.right, view.transform.up);
				gameObject3.transform.position += view.transform.right * view.Corpulence;
				break;
			case WeakpointSide.Back:
				gameObject3.transform.rotation = Quaternion.LookRotation(-view.transform.forward, view.transform.up);
				gameObject3.transform.position -= view.transform.forward * view.Corpulence;
				break;
			default:
				throw new ArgumentOutOfRangeException("side", side, null);
			}
		}
	}

	private void RemoveWeakpoint(UnitEntityView view, WeakpointSide side)
	{
		Entry entry = _entries.Get(view);
		if (entry != null)
		{
			GameObject gameObject = entry.Marks.Get(side);
			if ((bool)gameObject)
			{
				UnityEngine.Object.Destroy(gameObject);
				entry.Marks.Remove(side);
			}
		}
	}

	private GameObject GetWeakpointMarkPrefab(WeakpointSide side)
	{
		return WeakpointPrefab;
	}

	void IWeakpointAdded.HandleWeakpointAdded(WeakpointSide side)
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null)
		{
			IUnitEntityView view = baseUnitEntity.View;
			if (view != null)
			{
				UpdateWeakpoints(view.AsUnitEntityView());
			}
		}
	}

	void IWeakpointRemoved.HandleWeakpointRemoved(WeakpointSide side)
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null)
		{
			IUnitEntityView view = baseUnitEntity.View;
			if (view != null)
			{
				UpdateWeakpoints(view.AsUnitEntityView());
			}
		}
	}

	void IViewAttachedHandler.OnViewAttached(IEntityView view)
	{
		if (view is UnitEntityView view2)
		{
			UpdateWeakpoints(view2);
		}
	}
}
