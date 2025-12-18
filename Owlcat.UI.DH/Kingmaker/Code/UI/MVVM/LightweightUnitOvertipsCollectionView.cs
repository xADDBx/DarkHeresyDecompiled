using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA.Arbiter.Profiling;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LightweightUnitOvertipsCollectionView : View<LightweightUnitOvertipsCollectionVM>
{
	[SerializeField]
	private RectTransform m_NpcContainer;

	[SerializeField]
	private LightweightUnitOvertipView m_OvertipLightweightUnitView;

	private readonly Queue<MonoBehaviour> m_FreeNPCOvertips = new Queue<MonoBehaviour>();

	private readonly Dictionary<LightweightUnitOvertipVM, IBindable> m_ActiveOvertips = new Dictionary<LightweightUnitOvertipVM, IBindable>();

	private bool m_ClearDeadOvertips;

	public void Initialize()
	{
		OvertipUtils.PrewarmOvertips(m_OvertipLightweightUnitView, 10, m_NpcContainer);
	}

	private bool ExtraUnitShouldHaveOvertip(BaseUnitEntity unit)
	{
		PartUnitInteractions optional = unit.GetOptional<PartUnitInteractions>();
		if (optional == null || !optional.HasDialogInteractions)
		{
			return unit.Blueprint.GetComponent<AddLocalMapMarker>() != null;
		}
		return true;
	}

	public void Update()
	{
		if (base.ViewModel?.Overtips == null)
		{
			return;
		}
		using (Counters.Overtips?.Measure())
		{
			using (ProfileScope.New("Lightweight units VM visibility"))
			{
				for (int i = 0; i < base.ViewModel.Overtips.Count; i++)
				{
					LightweightUnitOvertipVM lightweightUnitOvertipVM = base.ViewModel.Overtips[i];
					MechanicEntity unit = lightweightUnitOvertipVM.Unit;
					bool flag = unit != null && (!unit.Features.IsUntargetable || lightweightUnitOvertipVM.IsBarkActive.CurrentValue) && (!(unit is BaseUnitEntity { IsExtra: not false } baseUnitEntity) || ExtraUnitShouldHaveOvertip(baseUnitEntity) || lightweightUnitOvertipVM.IsBarkActive.CurrentValue || baseUnitEntity.View.IsHighlighted || baseUnitEntity.View.MouseHoverHighlighting || lightweightUnitOvertipVM.HasSurrounding.CurrentValue) && unit.IsVisibleForPlayer && (lightweightUnitOvertipVM.ForceOnScreen || (!lightweightUnitOvertipVM.HideFromScreen && lightweightUnitOvertipVM.IsInCameraFrustum));
					bool flag2 = m_ActiveOvertips.Get(lightweightUnitOvertipVM) != null;
					if (flag != flag2)
					{
						if (flag)
						{
							AddOvertip(lightweightUnitOvertipVM);
						}
						else
						{
							RemoveOvertip(lightweightUnitOvertipVM);
						}
					}
				}
			}
			if (!m_ClearDeadOvertips)
			{
				return;
			}
			m_ClearDeadOvertips = false;
			using (ProfileScope.New("ClearDeadOvertips"))
			{
				foreach (var (lightweightUnitOvertipVM3, view) in m_ActiveOvertips)
				{
					if (lightweightUnitOvertipVM3 == null)
					{
						FreeOvertip(view);
					}
				}
			}
		}
	}

	protected override void OnBind()
	{
		base.ViewModel.Overtips.ObserveRemove().Subscribe(delegate
		{
			m_ClearDeadOvertips = true;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.Overtips.ObserveReset(), delegate
		{
			m_ClearDeadOvertips = true;
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		foreach (IBindable value in m_ActiveOvertips.Values)
		{
			FreeOvertip(value);
		}
		m_ActiveOvertips.Clear();
	}

	private T GetWidget<T>(Queue<MonoBehaviour> queue, T prefab, Transform targetContainer) where T : MonoBehaviour
	{
		if (queue.Count == 0)
		{
			T widget = WidgetFactory.GetWidget(prefab);
			widget.transform.SetParent(targetContainer, worldPositionStays: false);
			return widget;
		}
		return (T)queue.Dequeue();
	}

	private void AddOvertip(LightweightUnitOvertipVM vm)
	{
		if (!(m_OvertipLightweightUnitView == null))
		{
			Transform npcContainer = m_NpcContainer;
			LightweightUnitOvertipView widget = GetWidget(m_FreeNPCOvertips, m_OvertipLightweightUnitView, npcContainer);
			widget.Bind(vm);
			m_ActiveOvertips.Add(vm, widget);
		}
	}

	private void RemoveOvertip(LightweightUnitOvertipVM vm)
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
		m_FreeNPCOvertips.Enqueue(view as MonoBehaviour);
	}
}
