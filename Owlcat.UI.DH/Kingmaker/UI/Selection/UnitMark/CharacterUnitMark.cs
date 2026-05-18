using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Controllers.DetectiveRadar;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.Predictions;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UI.Selection.UnitMark;

public class CharacterUnitMark : BaseSurfaceUnitMark, INetRoleSetHandler, ISubscriber, INetStopPlayingHandler, INetPingEntity, IDetectiveRadarHandler, ICellAbilityHandler<EntitySubscriber>, ICellAbilityHandler, ISubscriber<IMechanicEntity>, IEntitySubscriber, IEventTag<ICellAbilityHandler, EntitySubscriber>, IAbilityTargetHoverUIHandler
{
	[Header("Exploration")]
	[SerializeField]
	private UnitMarkDecal m_ExplorationSelectedDecal;

	[SerializeField]
	private UnitMarkDecal m_ExplorationSignalSelectedDecal;

	[SerializeField]
	private UnitMarkDecal m_ExplorationOtherPlayerDecal;

	[SerializeField]
	private UnitMarkDecal m_ExplorationDialogCurrentSpeakerDecal;

	[Header("Combat")]
	[Header("Companions")]
	[SerializeField]
	private UnitMarkDecal m_CombatDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatSelectedDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatCurrentTurnDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatHeroicDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatSelectedHeroicDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatBrokenDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatSelectedBrokenDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatIsInAoeDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatAbilityTargetDecal;

	[Header("Other Player")]
	[SerializeField]
	private UnitMarkDecal m_CombatOtherPlayerDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatSelectedOtherPlayerDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatCurrentTurnOtherPlayerDecal;

	[Header("GamePad")]
	[SerializeField]
	private UnitMarkDecal m_GamepadSelectedDecal;

	[Header("Coop")]
	[SerializeField]
	private UnitMarkDecal m_PingTarget;

	private Tween m_PingTween;

	private bool m_SignalSource;

	protected override List<UnitMarkDecal> GetAllDecals()
	{
		return new List<UnitMarkDecal>
		{
			m_ExplorationSelectedDecal, m_ExplorationSignalSelectedDecal, m_ExplorationOtherPlayerDecal, m_ExplorationDialogCurrentSpeakerDecal, m_ExplorationDialogCurrentSpeakerDecal, m_CombatDecal, m_CombatSelectedDecal, m_CombatCurrentTurnDecal, m_CombatHeroicDecal, m_CombatSelectedHeroicDecal,
			m_CombatBrokenDecal, m_CombatSelectedBrokenDecal, m_CombatOtherPlayerDecal, m_CombatSelectedOtherPlayerDecal, m_CombatCurrentTurnOtherPlayerDecal, m_GamepadSelectedDecal
		};
	}

	protected override UnitMarkDecal GetAbilityTargetDecal()
	{
		return m_CombatAbilityTargetDecal;
	}

	public override void Initialize(AbstractUnitEntity unit)
	{
		base.Initialize(unit);
		SetUnitSize(unit.SizeRect.Width > 1);
		bool isSelected = Game.Instance.Controllers.SelectionCharacter.SelectedUnits.HasItem((BaseUnitEntity i) => i == unit);
		Selected(isSelected);
		m_SignalSource = (unit as BaseUnitEntity)?.IsMainCharacter ?? false;
		m_PingTarget?.SetActive(state: false);
		m_CombatIsInAoeDecal?.SetActive(state: false);
	}

	public override void HandleStateChanged()
	{
		if (base.Unit != null)
		{
			bool flag = base.State.HasFlag(UnitMarkState.CurrentTurn);
			bool flag2 = base.State.HasFlag(UnitMarkState.DialogCurrentSpeaker);
			bool flag3 = base.State.HasFlag(UnitMarkState.Selected);
			bool flag4 = base.State.HasFlag(UnitMarkState.IsInCombat);
			bool isDirectlyControllable = base.Unit.IsDirectlyControllable;
			bool flag5 = base.State.HasFlag(UnitMarkState.GamepadSelected);
			bool flag6 = base.State.HasFlag(UnitMarkState.Heroic);
			bool flag7 = base.State.HasFlag(UnitMarkState.Broken);
			bool flag8 = flag6 || flag7;
			bool flag9 = Game.Instance.Controllers.DetectiveRadarController.SignalState == DetectiveRadarState.Activated;
			bool flag10 = base.Unit.IsMyNetRole();
			bool inLobbyAndPlaying = UtilityNet.InLobbyAndPlaying;
			bool flag11 = BaseUnitMark.IsHideAllUI || IsHiddenBySettings();
			m_ExplorationOtherPlayerDecal.SetActive(!flag11 && !BaseUnitMark.IsCutscene && !flag2 && !flag4 && !flag10 && inLobbyAndPlaying);
			m_ExplorationSelectedDecal.SetActive(!flag11 && !BaseUnitMark.IsCutscene && !flag && !flag2 && flag3 && !flag4 && isDirectlyControllable);
			m_ExplorationSignalSelectedDecal.SetActive(!flag11 && !BaseUnitMark.IsCutscene && !flag && !flag2 && flag3 && !flag4 && isDirectlyControllable && m_SignalSource && flag9);
			m_ExplorationDialogCurrentSpeakerDecal.SetActive(!flag11 && !BaseUnitMark.IsCutscene && flag2);
			m_CombatDecal.SetActive(!flag11 && !BaseUnitMark.IsCutscene && flag4 && !flag && flag10 && !flag8);
			m_CombatSelectedDecal.SetActive(!flag11 && !BaseUnitMark.IsCutscene && flag4 && flag3 && flag && flag10 && !flag8);
			m_CombatHeroicDecal.SetActive(!flag11 && !BaseUnitMark.IsCutscene && flag4 && !flag && flag10 && flag6);
			m_CombatSelectedHeroicDecal.SetActive(!flag11 && !BaseUnitMark.IsCutscene && flag && flag10 && flag6);
			m_CombatBrokenDecal.SetActive(!flag11 && !BaseUnitMark.IsCutscene && flag4 && !flag && flag10 && flag7);
			m_CombatSelectedBrokenDecal.SetActive(!flag11 && !BaseUnitMark.IsCutscene && flag && flag10 && flag7);
			m_CombatCurrentTurnDecal.SetActive(!flag11 && !BaseUnitMark.IsCutscene && flag && flag10);
			m_CombatOtherPlayerDecal.SetActive(!flag11 && !BaseUnitMark.IsCutscene && flag4 && !flag && !flag10 && inLobbyAndPlaying && !flag8);
			m_CombatSelectedOtherPlayerDecal.SetActive(!flag11 && !BaseUnitMark.IsCutscene && flag4 && flag && !flag10 && Game.Instance.Controllers.TurnController?.CurrentUnit == base.Unit && inLobbyAndPlaying && !flag8);
			m_CombatCurrentTurnOtherPlayerDecal.SetActive(!flag11 && !BaseUnitMark.IsCutscene && flag && !flag10 && inLobbyAndPlaying);
			m_GamepadSelectedDecal.SetActive(!flag11 && !BaseUnitMark.IsCutscene && flag5);
		}
	}

	public override void SetGamepadSelected(bool selected)
	{
		SetState(UnitMarkState.GamepadSelected, selected);
	}

	public void HandleRoleSet(string entityId)
	{
		if (base.Unit != null && base.Unit.UniqueId == entityId)
		{
			HandleStateChanged();
		}
	}

	void INetStopPlayingHandler.HandleStopPlaying()
	{
		HandleStateChanged();
	}

	public void HandlePingEntity(NetPlayer player, Entity entity)
	{
		if (entity != base.Unit)
		{
			return;
		}
		m_PingTween?.Kill();
		int index = player.Index - 1;
		m_PingTarget.SetMaterial(ConfigRoot.Instance.UIConfig.CoopPlayersPingsMaterials[index]);
		m_PingTarget?.SetActive(state: true);
		EventBus.RaiseEvent(delegate(INetAddPingMarker h)
		{
			h.HandleAddPingEntityMarker(entity);
		});
		m_PingTween = DOTween.To(() => 1f, delegate
		{
		}, 0f, 7.5f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			m_PingTarget?.SetActive(state: false);
			EventBus.RaiseEvent(delegate(INetAddPingMarker h)
			{
				h.HandleRemovePingEntityMarker(entity);
			});
			m_PingTween = null;
		})
			.OnKill(delegate
			{
				m_PingTarget?.SetActive(state: false);
				EventBus.RaiseEvent(delegate(INetAddPingMarker h)
				{
					h.HandleRemovePingEntityMarker(entity);
				});
				m_PingTween = null;
			});
	}

	public void HandleRadarModeChange(DetectiveRadarState state)
	{
		HandleStateChanged();
	}

	public void HandleNearestSignalTurnedOn()
	{
	}

	public void HandleCellAbility(AbilityTargetUIData abilityTarget)
	{
		SetState(UnitMarkState.IsInAoEPattern, active: true);
	}

	public void HandleCellAbilityClear()
	{
		SetState(UnitMarkState.IsInAoEPattern, active: false);
	}

	protected override void HandleAbilityTargetSelectionStartImpl(AbilityData ability)
	{
	}

	protected override void HandleAbilityTargetSelectionEndImpl(AbilityData ability)
	{
		SetState(UnitMarkState.IsInAoEPattern, active: false);
	}

	public void HandleAbilityTargetHover(AbilityData ability, bool hover)
	{
		if (!hover)
		{
			SetState(UnitMarkState.IsInAoEPattern, active: false);
		}
	}
}
