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
			GameObject gameObject2 = (entry2.Marks[side] = UnityEngine.Object.Instantiate(GetWeakpointMarkPrefab(side), view.ViewTransform, worldPositionStays: true));
			GameObject gameObject3 = gameObject2;
			gameObject3.transform.position = view.ViewTransform.position;
			gameObject3.transform.rotation = view.ViewTransform.rotation;
			switch (side)
			{
			case WeakpointSide.Front:
				gameObject3.transform.position += view.ViewTransform.forward * view.Corpulence;
				break;
			case WeakpointSide.Left:
				gameObject3.transform.rotation = Quaternion.LookRotation(-view.ViewTransform.right, view.ViewTransform.up);
				gameObject3.transform.position -= view.ViewTransform.right * view.Corpulence;
				break;
			case WeakpointSide.Right:
				gameObject3.transform.rotation = Quaternion.LookRotation(view.ViewTransform.right, view.ViewTransform.up);
				gameObject3.transform.position += view.ViewTransform.right * view.Corpulence;
				break;
			case WeakpointSide.Back:
				gameObject3.transform.rotation = Quaternion.LookRotation(-view.ViewTransform.forward, view.ViewTransform.up);
				gameObject3.transform.position -= view.ViewTransform.forward * view.Corpulence;
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
			UnitEntityView view = baseUnitEntity.View;
			if ((object)view != null)
			{
				UpdateWeakpoints(view);
			}
		}
	}

	void IWeakpointRemoved.HandleWeakpointRemoved(WeakpointSide side)
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null)
		{
			UnitEntityView view = baseUnitEntity.View;
			if ((object)view != null)
			{
				UpdateWeakpoints(view);
			}
		}
	}

	void IViewAttachedHandler.OnViewAttached(IEntityViewBase view)
	{
		if (view is UnitEntityView view2)
		{
			UpdateWeakpoints(view2);
		}
	}
}
