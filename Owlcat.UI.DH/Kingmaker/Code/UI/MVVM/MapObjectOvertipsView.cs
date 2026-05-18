using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA.Arbiter.Profiling;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class MapObjectOvertipsView : View<MapObjectOvertipsVM>
{
	[SerializeField]
	private RectTransform m_TargetContainer;

	[SerializeField]
	private OvertipTransitionView m_OvertipTransitionView;

	[SerializeField]
	private OvertipMapObjectSimpleView m_OvertipMapObjectSimpleView;

	[SerializeField]
	private OvertipMapObjectInteractionView m_OvertipMapObjectInteractionView;

	[SerializeField]
	private OvertipDestructibleObjectView m_OvertipDestructibleObjectView;

	private readonly Queue<MonoBehaviour> m_FreeTransitionOvertips = new Queue<MonoBehaviour>();

	private readonly Queue<MonoBehaviour> m_FreeMapObjectSimpleOvertips = new Queue<MonoBehaviour>();

	private readonly Queue<MonoBehaviour> m_FreeMapObjectInteractionOvertips = new Queue<MonoBehaviour>();

	private readonly Queue<MonoBehaviour> m_FreeDestructibleObjectOvertips = new Queue<MonoBehaviour>();

	private readonly Dictionary<BaseOvertipMapObjectVM, IBindable> m_ActiveOvertips = new Dictionary<BaseOvertipMapObjectVM, IBindable>();

	private bool m_ClearDeadOvertips;

	private Action<OvertipTransitionVM> m_AddTransition;

	private Action<OvertipTransitionVM> m_RemoveTransition;

	private Action<OvertipMapObjectVM> m_AddMapObject;

	private Action<OvertipMapObjectVM> m_RemoveMapObject;

	private Action<OvertipDestructibleObjectVM> m_AddDestructible;

	private Action<OvertipDestructibleObjectVM> m_RemoveDestructible;

	private Func<BaseOvertipMapObjectVM, bool> m_IsObjectVisibleWithFrustrumCheck;

	private Func<BaseOvertipMapObjectVM, bool> m_IsObjectVisibleNoFrustrum;

	public void Initialize()
	{
		m_AddTransition = AddTransition;
		m_RemoveTransition = RemoveTransition;
		m_AddMapObject = AddMapObject;
		m_RemoveMapObject = RemoveMapObject;
		m_AddDestructible = AddDestructibleObject;
		m_RemoveDestructible = RemoveDestructibleObject;
		m_IsObjectVisibleWithFrustrumCheck = IsObjectVisible;
		m_IsObjectVisibleNoFrustrum = (BaseOvertipMapObjectVM vm) => IsObjectVisible(vm, checkFrustum: false);
		OvertipUtils.PrewarmOvertips(m_OvertipTransitionView, 5, m_TargetContainer);
		OvertipUtils.PrewarmOvertips(m_OvertipMapObjectSimpleView, 15, m_TargetContainer);
		OvertipUtils.PrewarmOvertips(m_OvertipMapObjectInteractionView, 15, m_TargetContainer);
	}

	protected override void OnBind()
	{
		if (base.ViewModel.TransitionOvertipsCollectionVM != null)
		{
			base.ViewModel.TransitionOvertipsCollectionVM.Overtips.ObserveRemove().Subscribe(delegate
			{
				m_ClearDeadOvertips = true;
			}).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(base.ViewModel.TransitionOvertipsCollectionVM.Overtips.ObserveReset(), delegate
			{
				m_ClearDeadOvertips = true;
			}).AddTo(this);
		}
		if (base.ViewModel.MapInteractionObjectOvertipsCollectionVM.Overtips != null)
		{
			base.ViewModel.MapInteractionObjectOvertipsCollectionVM.Overtips.ObserveRemove().Subscribe(delegate
			{
				m_ClearDeadOvertips = true;
			}).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(base.ViewModel.MapInteractionObjectOvertipsCollectionVM.Overtips.ObserveReset(), delegate
			{
				m_ClearDeadOvertips = true;
			}).AddTo(this);
		}
		if (base.ViewModel.DestructibleObjectOvertipsCollectionVM != null)
		{
			base.ViewModel.DestructibleObjectOvertipsCollectionVM.Overtips.ObserveRemove().Subscribe(delegate
			{
				m_ClearDeadOvertips = true;
			}).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(base.ViewModel.DestructibleObjectOvertipsCollectionVM.Overtips.ObserveReset(), delegate
			{
				m_ClearDeadOvertips = true;
			}).AddTo(this);
		}
	}

	protected override void OnUnbind()
	{
		foreach (IBindable value in m_ActiveOvertips.Values)
		{
			FreeOvertip(value);
		}
		m_ActiveOvertips.Clear();
	}

	public void LateUpdate()
	{
		using (Counters.Overtips?.Measure())
		{
			using (ProfileScope.New("Objects VM visibility"))
			{
				ProcessTransitions();
				ProcessMapInteractionObjects();
				ProcessDestructibleObjects();
			}
			ClearDeadOvertips();
		}
	}

	private void ProcessTransitions()
	{
		using (ProfileScope.New("TransitionOvertipsCollectionVM"))
		{
			if (base.ViewModel?.TransitionOvertipsCollectionVM != null)
			{
				ProcessCollection(base.ViewModel.TransitionOvertipsCollectionVM.Overtips, m_IsObjectVisibleWithFrustrumCheck, m_AddTransition, m_RemoveTransition);
			}
		}
	}

	private void ProcessMapInteractionObjects()
	{
		using (ProfileScope.New("MapInteractionObjectOvertipsCollectionVM"))
		{
			if (base.ViewModel?.MapInteractionObjectOvertipsCollectionVM != null)
			{
				ProcessCollection(base.ViewModel.MapInteractionObjectOvertipsCollectionVM.Overtips, m_IsObjectVisibleWithFrustrumCheck, m_AddMapObject, m_RemoveMapObject);
			}
		}
	}

	private void ProcessDestructibleObjects()
	{
		using (ProfileScope.New("DestructibleObjectOvertipsCollectionVM"))
		{
			if (base.ViewModel?.DestructibleObjectOvertipsCollectionVM != null)
			{
				ProcessCollection(base.ViewModel.DestructibleObjectOvertipsCollectionVM.Overtips, m_IsObjectVisibleNoFrustrum, m_AddDestructible, m_RemoveDestructible);
			}
		}
	}

	private void ProcessCollection<T>(ObservableList<T> overtips, Func<T, bool> getVisibility, Action<T> addAction, Action<T> removeAction) where T : BaseOvertipMapObjectVM
	{
		for (int i = 0; i < overtips.Count; i++)
		{
			T val = overtips[i];
			bool flag = m_ActiveOvertips.Get(val) != null;
			bool flag2 = getVisibility(val);
			if (flag2 != flag)
			{
				if (flag2)
				{
					addAction(val);
				}
				else
				{
					removeAction(val);
				}
			}
		}
	}

	private bool IsObjectVisible(BaseOvertipMapObjectVM vm, bool checkFrustum)
	{
		MapObjectEntity mapObjectEntity = vm.MapObjectEntity;
		if (mapObjectEntity != null && mapObjectEntity.IsVisibleForPlayer && !vm.IsMarkedForRemoval && !vm.HideFromScreen)
		{
			if (checkFrustum)
			{
				return vm.IsInCameraFrustum;
			}
			return true;
		}
		return false;
	}

	private bool IsObjectVisible(BaseOvertipMapObjectVM vm)
	{
		return IsObjectVisible(vm, checkFrustum: true);
	}

	private void ClearDeadOvertips()
	{
		if (!m_ClearDeadOvertips)
		{
			return;
		}
		m_ClearDeadOvertips = false;
		using (ProfileScope.New("ClearDeadOvertips"))
		{
			foreach (var (baseOvertipMapObjectVM2, view) in m_ActiveOvertips)
			{
				if (baseOvertipMapObjectVM2 == null)
				{
					FreeOvertip(view);
				}
			}
		}
	}

	private T GetWidget<T>(Queue<MonoBehaviour> queue, T prefab) where T : MonoBehaviour
	{
		if (queue.Count == 0)
		{
			T widget = WidgetFactory.GetWidget(prefab);
			widget.transform.SetParent(m_TargetContainer, worldPositionStays: false);
			return widget;
		}
		return (T)queue.Dequeue();
	}

	private void AddTransition(OvertipTransitionVM transitionVM)
	{
		OvertipTransitionView widget = GetWidget(m_FreeTransitionOvertips, m_OvertipTransitionView);
		widget.Bind(transitionVM);
		m_ActiveOvertips.Add(transitionVM, widget);
	}

	private void RemoveTransition(OvertipTransitionVM vm)
	{
		IBindable bindable = m_ActiveOvertips.Get(vm);
		if (bindable != null)
		{
			m_ActiveOvertips.Remove(vm);
			FreeOvertip(bindable);
		}
	}

	private void AddMapObject(OvertipMapObjectVM mapObjectVM)
	{
		Queue<MonoBehaviour> queue = (mapObjectVM.HasInteractionsWithOvertip ? m_FreeMapObjectInteractionOvertips : m_FreeMapObjectSimpleOvertips);
		BaseOvertipMapObjectView prefab = (mapObjectVM.HasInteractionsWithOvertip ? ((BaseOvertipMapObjectView)m_OvertipMapObjectInteractionView) : ((BaseOvertipMapObjectView)m_OvertipMapObjectSimpleView));
		BaseOvertipMapObjectView widget = GetWidget(queue, prefab);
		widget.Bind(mapObjectVM);
		m_ActiveOvertips.Add(mapObjectVM, widget);
	}

	private void RemoveMapObject(OvertipMapObjectVM vm)
	{
		IBindable bindable = m_ActiveOvertips.Get(vm);
		if (bindable != null)
		{
			m_ActiveOvertips.Remove(vm);
			FreeOvertip(bindable);
		}
	}

	private void AddDestructibleObject(OvertipDestructibleObjectVM destructibleVM)
	{
		OvertipDestructibleObjectView widget = GetWidget(m_FreeDestructibleObjectOvertips, m_OvertipDestructibleObjectView);
		widget.Bind(destructibleVM);
		m_ActiveOvertips.Add(destructibleVM, widget);
	}

	private void RemoveDestructibleObject(OvertipDestructibleObjectVM vm)
	{
		IBindable bindable = m_ActiveOvertips.Get(vm);
		if (bindable != null)
		{
			m_ActiveOvertips.Remove(vm);
			FreeOvertip(bindable);
		}
	}

	private void FreeOvertip(IBindable view)
	{
		view.Unbind();
		if (view is MonoBehaviour item)
		{
			((view is OvertipTransitionView) ? m_FreeTransitionOvertips : ((view is OvertipMapObjectSimpleView) ? m_FreeMapObjectSimpleOvertips : ((view is OvertipMapObjectInteractionView) ? m_FreeMapObjectInteractionOvertips : ((!(view is OvertipDestructibleObjectView)) ? null : m_FreeDestructibleObjectOvertips))))?.Enqueue(item);
		}
	}
}
