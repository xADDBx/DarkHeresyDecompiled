using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.AreaLogic;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class VeilThicknessVM : ViewModel, IVeilDamageHandler, ISubscriber, IAbilityTargetSelectionUIHandler, IAbilityTargetHoverUIHandler, ITurnBasedModeHandler, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IGameModeHandler, IUnitBuffHandler, ISubscriber<IBaseUnitEntity>, IUnitCommandStartHandler, IUnitCommandEndHandler, IUnitCommandActHandler
{
	private readonly ReactiveProperty<int> m_Value = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_PredictedValue = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_PredictedDeltaValue = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_PerilsOfTheWarpChance = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<bool> m_IsTurnBasedActive = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsPlayerTurn = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsAppropriateGameMode = new ReactiveProperty<bool>();

	public readonly ObservableList<IUIDataProviderVM> VeilBuffVMs = new ObservableList<IUIDataProviderVM>();

	private readonly List<IUIDataProviderVM> m_RemoveVMs = new List<IUIDataProviderVM>();

	public TooltipTemplateVail Tooltip = new TooltipTemplateVail();

	private AbilityData m_SelectedAbility;

	public ReadOnlyReactiveProperty<int> Value => m_Value;

	public ReadOnlyReactiveProperty<int> PredictedValue => m_PredictedValue;

	public ReadOnlyReactiveProperty<int> PredictedDeltaValue => m_PredictedDeltaValue;

	public ReadOnlyReactiveProperty<int> PerilsOfTheWarpChance => m_PerilsOfTheWarpChance;

	public ReadOnlyReactiveProperty<bool> IsTurnBasedActive => m_IsTurnBasedActive;

	public ReadOnlyReactiveProperty<bool> IsPlayerTurn => m_IsPlayerTurn;

	public ReadOnlyReactiveProperty<bool> IsAppropriateGameMode => m_IsAppropriateGameMode;

	public PartVeil Veil => Game.Instance.LoadedArea?.Veil;

	public VeilThicknessVM()
	{
		EventBus.Subscribe(this).AddTo(this);
		m_Value.Value = Game.Instance.LoadedArea.Veil.Damage;
		Value.Subscribe(SetTooltipValues).AddTo(this);
		OnGameModeStart(Game.Instance.CurrentModeType);
		if (TurnController.IsInTurnBasedCombat())
		{
			HandleTurnBasedModeResumed();
		}
		UpdateVeilBuffs();
	}

	public void HandleVeilDamageChanged(int delta, int value)
	{
		ReactiveProperty<int> predictedValue = m_PredictedValue;
		int value2 = (m_Value.Value = value);
		predictedValue.Value = value2;
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		m_SelectedAbility = ability;
		UpdateValues(ability);
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		m_SelectedAbility = null;
		UpdateValues(null);
	}

	public void HandleAbilityTargetHover(AbilityData ability, bool hover)
	{
		UpdateValues((!hover) ? m_SelectedAbility : ability);
	}

	private void UpdateValues([CanBeNull] AbilityData ability)
	{
		if (ability == null)
		{
			m_PredictedValue.Value = Value.CurrentValue;
			m_PredictedDeltaValue.Value = 0;
		}
		else
		{
			int predictedVeilDelta = ability.GetPredictedVeilDelta();
			m_PredictedValue.Value = Value.CurrentValue + predictedVeilDelta;
			m_PredictedDeltaValue.Value = predictedVeilDelta;
		}
	}

	private void SetTooltipValues(int value)
	{
		Tooltip.ChangeValue(value, Veil?.PerilsOfTheWarpChance ?? 0);
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		m_IsTurnBasedActive.Value = isTurnBased;
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased)
		{
			m_IsPlayerTurn.Value = Game.Instance.Controllers.TurnController.IsPlayerTurn;
		}
	}

	public void HandleTurnBasedModeResumed()
	{
		m_IsTurnBasedActive.Value = true;
		m_IsPlayerTurn.Value = Game.Instance.Controllers.TurnController.IsPlayerTurn;
		UpdateVeilBuffs();
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		m_IsAppropriateGameMode.Value = gameMode != GameModeType.Dialog && gameMode != GameModeType.Cutscene;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void Update()
	{
		OnGameModeStart(Game.Instance.CurrentModeType);
		m_IsTurnBasedActive.Value = TurnController.IsInTurnBasedCombat();
		m_IsPlayerTurn.Value = Game.Instance.Controllers.TurnController.IsPlayerTurn;
		UpdateVeilBuffs();
	}

	private void UpdateVeilBuffs()
	{
		GameUIState instance = GameUIState.Instance;
		if (instance != null && !instance.IsInCombat.Value)
		{
			VeilBuffVMs.Clear();
			return;
		}
		if (Game.Instance?.Controllers?.TurnController?.CurrentUnit == null)
		{
			VeilBuffVMs.Clear();
			return;
		}
		m_RemoveVMs.AddRange(VeilBuffVMs.ToList());
		Game.Instance.Controllers.TurnController.CurrentUnit.GetMechanicFeature(MechanicsFeatureType.PsykerFetter).AssociatedFacts.Elements.ForEach(delegate(FeatureCountableFlag.FactsList.Element f)
		{
			TryAddFeature(f.Fact);
		});
		Game.Instance.Controllers.TurnController.CurrentUnit.GetMechanicFeature(MechanicsFeatureType.PsykerPush).AssociatedFacts.Elements.ForEach(delegate(FeatureCountableFlag.FactsList.Element f)
		{
			TryAddFeature(f.Fact);
		});
		m_RemoveVMs.ForEach(delegate
		{
			VeilBuffVMs.Remove((IUIDataProviderVM vm) => m_RemoveVMs.Contains(vm));
		});
		m_RemoveVMs.Clear();
	}

	private void TryAddFeature(EntityFact entityFact)
	{
		if (entityFact == null)
		{
			return;
		}
		if (VeilBuffVMs.FirstOrDefault((IUIDataProviderVM vm) => vm.IUIDataProvider.Equals(entityFact)) != null)
		{
			m_RemoveVMs.Remove((IUIDataProviderVM vm) => vm.IUIDataProvider.Equals(entityFact));
		}
		else
		{
			VeilBuffVMs.Add(new IUIDataProviderVM(entityFact));
		}
	}

	public void HandleBuffDidAdded(Buff buff, MechanicEntity caster)
	{
		UpdateVeilBuffs();
	}

	public void HandleBuffDidRemoved(Buff buff, MechanicEntity caster)
	{
		UpdateVeilBuffs();
	}

	public void HandleBuffRankIncreased(Buff buff, int delta, MechanicEntity caster)
	{
		UpdateVeilBuffs();
	}

	public void HandleBuffRankDecreased(Buff buff, int delta, MechanicEntity caster)
	{
		UpdateVeilBuffs();
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		UpdateValues(null);
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		UpdateValues(null);
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		UpdateValues(null);
	}
}
