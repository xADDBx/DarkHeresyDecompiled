using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Code.Gameplay.Components;
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

public class UnitOvertipsView : View<UnitOvertipsCollectionVM>
{
	[SerializeField]
	private RectTransform m_PartyContainer;

	[SerializeField]
	private RectTransform m_EnemyContainer;

	[SerializeField]
	private RectTransform m_NpcContainer;

	[SerializeField]
	private OvertipUnitView m_OvertipUnitView;

	private readonly Queue<MonoBehaviour> m_FreePartyOvertips = new Queue<MonoBehaviour>();

	private readonly Queue<MonoBehaviour> m_FreeEnemyOvertips = new Queue<MonoBehaviour>();

	private readonly Queue<MonoBehaviour> m_FreeNPCOvertips = new Queue<MonoBehaviour>();

	private readonly Dictionary<OvertipUnitVM, OvertipUnitView> m_ActiveOvertips = new Dictionary<OvertipUnitVM, OvertipUnitView>();

	private bool m_ClearDeadOvertips;

	private MechanicEntity m_IteractedUnit;

	private bool m_ShouldBeVisible;

	private bool m_IsVisibleNow;

	public void Initialize()
	{
		OvertipUtils.PrewarmOvertips(m_OvertipUnitView, 6, m_PartyContainer);
		OvertipUtils.PrewarmOvertips(m_OvertipUnitView, 10, m_EnemyContainer);
		OvertipUtils.PrewarmOvertips(m_OvertipUnitView, 10, m_NpcContainer);
	}

	public void LateUpdate()
	{
		if (base.ViewModel?.Overtips == null)
		{
			return;
		}
		int num = 1;
		int num2 = 0;
		using (Counters.Overtips?.Measure())
		{
			using (ProfileScope.New("Units VM visibility"))
			{
				for (int i = 0; i < base.ViewModel.Overtips.Count; i++)
				{
					OvertipUnitVM overtipUnitVM = base.ViewModel.Overtips[i];
					m_IteractedUnit = overtipUnitVM.Unit;
					m_ShouldBeVisible = IsNeedToShowUnitOvertip(overtipUnitVM);
					OvertipUnitView overtipUnitView = m_ActiveOvertips.Get(overtipUnitVM);
					m_IsVisibleNow = overtipUnitView != null;
					if (m_ShouldBeVisible == m_IsVisibleNow)
					{
						continue;
					}
					if (m_ShouldBeVisible)
					{
						if (num2 >= num)
						{
							break;
						}
						num2++;
						overtipUnitView = AddOvertip(overtipUnitVM);
					}
					else
					{
						RemoveOvertip(overtipUnitVM);
					}
				}
				m_IteractedUnit = null;
				m_ShouldBeVisible = false;
				m_IsVisibleNow = false;
			}
			if (!m_ClearDeadOvertips)
			{
				return;
			}
			m_ClearDeadOvertips = false;
			using (ProfileScope.New("ClearDeadOvertips"))
			{
				foreach (var (overtipUnitVM3, view) in m_ActiveOvertips)
				{
					if (overtipUnitVM3 == null)
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
		foreach (OvertipUnitView value in m_ActiveOvertips.Values)
		{
			FreeOvertip(value);
		}
		m_ActiveOvertips.Clear();
	}

	private bool IsNeedToShowUnitOvertip(OvertipUnitVM vm)
	{
		if (vm.HideFromScreen)
		{
			return false;
		}
		bool currentValue = vm.IsBarkActive.CurrentValue;
		bool currentValue2 = vm.HasActiveCombatMessage.CurrentValue;
		if (currentValue || currentValue2)
		{
			return true;
		}
		if (!CanShowUnitOvertip(vm.Unit))
		{
			return false;
		}
		if (!vm.IsInCameraFrustum && !vm.MechanicEntityUIState.IsInCombat.CurrentValue)
		{
			return false;
		}
		bool currentValue3 = vm.HasCombatInteraction.CurrentValue;
		bool currentValue4 = vm.HasSurrounding.CurrentValue;
		MechanicEntity unit = vm.Unit;
		return (unit != null && !unit.IsDisposed && !unit.IsDead) || currentValue3 || currentValue4;
	}

	private bool CanShowUnitOvertip(MechanicEntity unit)
	{
		if (unit == null)
		{
			return false;
		}
		if ((bool)unit.Features.IsUntargetable)
		{
			UnitUISettings component = unit.Blueprint.GetComponent<UnitUISettings>();
			if (component != null && component.OvertipSettings.ShowForUntargetable)
			{
				return true;
			}
		}
		if (unit is BaseUnitEntity baseUnitEntity && (ForceShowExtraUnitOvertip(baseUnitEntity) || (baseUnitEntity.View != null && (baseUnitEntity.View.IsHighlighted || baseUnitEntity.View.MouseHoverHighlighting))))
		{
			return true;
		}
		return !unit.Features.IsUntargetable;
	}

	private bool ForceShowExtraUnitOvertip(BaseUnitEntity unit)
	{
		if (!unit.IsExtra)
		{
			return false;
		}
		PartUnitInteractions optional = unit.GetOptional<PartUnitInteractions>();
		if (optional == null || !optional.HasDialogInteractions)
		{
			return unit.Blueprint.GetComponent<AddLocalMapMarker>() != null;
		}
		return true;
	}

	private T GetWidget<T>(Queue<MonoBehaviour> queue, T prefab, Transform targetContainer) where T : MonoBehaviour
	{
		using (ProfileScope.New("GetWidget"))
		{
			if (queue.Count == 0)
			{
				T widget = WidgetFactory.GetWidget(prefab);
				widget.transform.SetParent(targetContainer, worldPositionStays: false);
				return widget;
			}
			return (T)queue.Dequeue();
		}
	}

	private OvertipUnitView AddOvertip(OvertipUnitVM vm)
	{
		using (ProfileScope.New("Add Ovetip OvertipEntityUnitVM"))
		{
			if (m_OvertipUnitView == null)
			{
				return null;
			}
			Transform targetContainer = (vm.MechanicEntityUIWrapper.IsPlayerEnemy ? m_EnemyContainer : (vm.MechanicEntityUIWrapper.IsPlayer ? m_PartyContainer : m_NpcContainer));
			OvertipUnitView overtipUnitView = (vm.MechanicEntityUIWrapper.IsPlayerEnemy ? GetWidget(m_FreeEnemyOvertips, m_OvertipUnitView, targetContainer) : (vm.MechanicEntityUIWrapper.IsPlayer ? GetWidget(m_FreePartyOvertips, m_OvertipUnitView, targetContainer) : GetWidget(m_FreeNPCOvertips, m_OvertipUnitView, targetContainer)));
			overtipUnitView.Bind(vm);
			m_ActiveOvertips.Add(vm, overtipUnitView);
			return overtipUnitView;
		}
	}

	private void RemoveOvertip(OvertipUnitVM vm)
	{
		using (ProfileScope.New("Remove Ovetip OvertipEntityUnitVM"))
		{
			OvertipUnitView overtipUnitView = m_ActiveOvertips.Get(vm);
			if (!(overtipUnitView == null))
			{
				m_ActiveOvertips.Remove(vm);
				FreeOvertip(overtipUnitView);
			}
		}
	}

	private void FreeOvertip(IBindable view)
	{
		view.Unbind();
		if (view is MonoBehaviour monoBehaviour)
		{
			Transform parent = monoBehaviour.transform.parent;
			((parent == m_EnemyContainer) ? m_FreeEnemyOvertips : ((parent == m_PartyContainer) ? m_FreePartyOvertips : m_FreeNPCOvertips)).Enqueue(monoBehaviour);
		}
	}
}
