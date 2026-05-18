using System;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.MVVM.ServiceWindows;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.StatefulRandom;
using R3;

namespace Kingmaker.Code.View.UI.UIUtilities;

public class CalculatorUnitPair : IDisposable, ILevelUpCompleteUIHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	private readonly ReactiveProperty<BaseUnitEntity> m_SelectedUnit;

	private readonly CompositeDisposable m_Disposables = new CompositeDisposable();

	public BaseUnitEntity CalculatorUnit { get; private set; }

	public BaseUnitEntity CurrentSelectedUnit => m_SelectedUnit.Value;

	public CalculatorUnitPair(ReactiveProperty<BaseUnitEntity> selectedUnit)
	{
		m_SelectedUnit = selectedUnit;
		m_SelectedUnit.Subscribe(delegate
		{
			UpdateCalculator(withBuffs: true);
		}).AddTo(m_Disposables);
		RootVM.Instance.WindowsPanelVM.Subscribe(delegate(ServiceWindowsPanelVM value)
		{
			if (value != null)
			{
				UpdateCalculator(withBuffs: true);
			}
		}).AddTo(m_Disposables);
		EventBus.Subscribe(this).AddTo(m_Disposables);
	}

	private void UpdateCalculator(bool withBuffs)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			using (ContextData<UnitHelper.DoNotCreateItems>.Request())
			{
				using (ContextData<UnitHelper.PreviewUnit>.Request())
				{
					CalculatorUnit?.Dispose();
					try
					{
						using (ContextData<ItemSlot.IgnoreLock>.Request())
						{
							using (ContextData<GameCommandHelper.PreviewItem>.Request())
							{
								BaseUnitEntity unit = m_SelectedUnit.Value ?? Game.Instance.Player.MainCharacterEntity;
								CalculatorUnit = unit.Copy(createView: false, preview: true, copyItems: true, withBuffs);
							}
						}
					}
					catch (Exception arg)
					{
						PFLog.UI.Error($"Cannot update calculator unit: {arg}");
					}
				}
			}
		}
	}

	public void Dispose()
	{
		CalculatorUnit?.Dispose();
		m_Disposables?.Dispose();
	}

	public void HandleLevelUpComplete()
	{
		UpdateCalculator(withBuffs: false);
	}
}
