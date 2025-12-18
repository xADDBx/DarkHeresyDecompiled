using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class UnitBuffBlockVM : ViewModel, IUnitBuffHandler<EntitySubscriber>, IEventTag<IUnitBuffHandler, EntitySubscriber>, IUnitBuffHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEntitySubscriber, ITurnStartHandler, ISubscriber<IMechanicEntity>, ICriticalEffectStageChanged
{
	private readonly ReactiveCommand<Unit> m_CheckSpecialComplete = new ReactiveCommand<Unit>();

	private readonly ObservableList<BuffVM> m_Buffs = new ObservableList<BuffVM>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_BuffsTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private IDisposable m_Subscription;

	private EntityStatusEffectsVM m_StatusEffects;

	private EntityCriticalEffectsVM m_CriticalEffects;

	private EntityDOTEffectsVM m_DotEffects;

	private MechanicEntity m_Unit;

	public MechanicEntityUIState MechanicEntityUIState { get; private set; }

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> BuffsTooltip => m_BuffsTooltip;

	public Observable<Unit> CheckSpecialComplete => m_CheckSpecialComplete;

	public IObservableCollection<BuffVM> Buffs => m_Buffs;

	public ReadOnlyReactiveProperty<StatusEffectsUIData> StatusEffects => m_StatusEffects.StatusEffects;

	public ReadOnlyReactiveProperty<CriticalEffectsUIData> CriticalEffects => m_CriticalEffects.CriticalEffectsData;

	public ReadOnlyReactiveProperty<DOTEffectsUIData> DOTEffects => m_DotEffects.DOTEffects;

	public UnitBuffBlockVM(MechanicEntity unit)
	{
		m_StatusEffects = new EntityStatusEffectsVM().AddTo(this);
		m_CriticalEffects = new EntityCriticalEffectsVM().AddTo(this);
		m_DotEffects = new EntityDOTEffectsVM().AddTo(this);
		if (unit == null)
		{
			ClearBuffs();
		}
		else
		{
			SetUnitData(unit);
		}
	}

	public IEntity GetSubscribingEntity()
	{
		return m_Unit;
	}

	public void SetUnitData(MechanicEntity unit)
	{
		m_Subscription?.Dispose();
		m_BuffsTooltip.Value = null;
		MechanicEntityUIState = null;
		m_Unit = unit;
		if (unit != null)
		{
			MechanicEntityUIState = UnitUIStateHolder.Instance.GetOrCreateUnitState(unit);
			m_BuffsTooltip.Value = new TooltipTemplateBuffs(m_Unit);
			m_StatusEffects.SetTargetEntity(unit);
			m_CriticalEffects.SetTargetEntity(unit);
			m_DotEffects.SetTargetEntity(unit);
			m_Subscription = EventBus.Subscribe(this);
		}
		UpdateData();
	}

	public void ClearBuffs()
	{
		m_Buffs.ForEach(delegate(BuffVM vm)
		{
			vm.Dispose();
		});
		m_Buffs.Clear();
	}

	public void SortBuffs()
	{
		List<BuffVM> list = Buffs.OrderBy((BuffVM b) => b.SortOrder).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			m_Buffs.Move(Buffs.IndexOf(list[i]), i);
		}
	}

	public void UpdateData()
	{
		ClearBuffs();
		if (m_Unit == null)
		{
			return;
		}
		foreach (Buff buff in m_Unit.Buffs)
		{
			if (!buff.Blueprint.IsHiddenInUI && !Buffs.Any((BuffVM b) => b.Buff == buff))
			{
				m_Buffs.Add(new BuffVM(buff));
			}
		}
	}

	void IUnitBuffHandler.HandleBuffDidAdded(Buff buff, MechanicEntity caster)
	{
		if (m_Unit != null && m_Unit == buff.Owner)
		{
			if (!buff.Blueprint.IsHiddenInUI)
			{
				m_Buffs.Add(new BuffVM(buff));
			}
			m_DotEffects.HandleBuffAdded(buff);
			m_StatusEffects.HandleBuffAdded(buff);
		}
	}

	void IUnitBuffHandler.HandleBuffDidRemoved(Buff buff, MechanicEntity caster)
	{
		if (m_Unit != null && m_Unit == buff.Owner)
		{
			BuffVM buffVM = Buffs.FirstOrDefault((BuffVM b) => b.Buff == buff);
			if (buffVM != null)
			{
				buffVM.Dispose();
				m_Buffs.Remove(buffVM);
			}
			m_DotEffects.HandleBuffRemoved(buff);
			m_StatusEffects.HandleBuffRemoved(buff);
		}
	}

	void IUnitBuffHandler.HandleBuffRankIncreased(Buff buff, int delta, MechanicEntity caster)
	{
		m_DotEffects.HandleBuffRankIncreased(buff);
	}

	void IUnitBuffHandler.HandleBuffRankDecreased(Buff buff, int delta, MechanicEntity caster)
	{
		m_DotEffects.HandleBuffRankDecreased(buff);
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			return;
		}
		foreach (BuffVM buff in Buffs)
		{
			buff.CheckSpecial();
		}
		m_CheckSpecialComplete.Execute();
	}

	void ICriticalEffectStageChanged.HandleCriticalEffectStageChanged(BlueprintBodyPart bodyPart, int previous, int current)
	{
		if (m_Unit == EventInvokerExtensions.MechanicEntity)
		{
			m_CriticalEffects.HandleCriticalEffectStageChanged(bodyPart, current);
		}
	}

	protected override void OnDispose()
	{
		m_Subscription?.Dispose();
		m_Subscription = null;
		ClearBuffs();
	}
}
