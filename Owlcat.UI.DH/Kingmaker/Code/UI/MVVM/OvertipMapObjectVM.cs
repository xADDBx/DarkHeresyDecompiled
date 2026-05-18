using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.Gameplay.Features.VariableInteractions;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Interaction;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipMapObjectVM : BaseOvertipMapObjectVM
{
	private readonly float m_ProximityRadius;

	private IDisposable m_SelectedUnitsSubscription;

	private readonly List<BaseUnitEntity> m_TryApproachingUnits = new List<BaseUnitEntity>();

	private readonly Dictionary<BaseUnitEntity, ForcedPath> m_CurrentlyApproachingUnits = new Dictionary<BaseUnitEntity, ForcedPath>();

	private readonly ReactiveProperty<string> m_Name = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<string> m_ObjectDescription = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<string> m_ObjectSkillCheckText = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<Vector3> m_CameraDistance = new ReactiveProperty<Vector3>(Vector3.zero);

	private readonly ReactiveProperty<bool> m_HasAdditionalCombatObjective = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<AdditionalCombatHintVM> m_AddCombatInfo = new ReactiveProperty<AdditionalCombatHintVM>();

	private readonly ReactiveProperty<int?> m_HasResourceCount = new ReactiveProperty<int?>();

	private readonly ReactiveProperty<bool> m_CanInteract = new ReactiveProperty<bool>(value: true);

	private readonly ReactiveProperty<bool> m_IsVariative = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<OvertipState> m_CurrentState = new ReactiveProperty<OvertipState>();

	private readonly ReactiveCommand<Unit> m_InventoryChanged = new ReactiveCommand<Unit>();

	private readonly ReactiveProperty<bool> m_ForceHotKeyPressed = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ForceHideInCombat = new ReactiveProperty<bool>();

	private readonly ReactiveCommand<Unit> m_CombatObjStateChanged = new ReactiveCommand<Unit>();

	public readonly OvertipBarkBlockVM BarkBlockVM;

	public readonly AbstractInteractionPart FirstInteractionPart;

	public readonly bool HasInteractions;

	public readonly bool HasInteractionsWithOvertip;

	public readonly bool CanBeForceShown;

	public int? RequiredResourceCount;

	public string ResourceName;

	public bool NotAvailable;

	public bool HasNewVariants;

	public bool ActiveCharacterIsNear { get; private set; }

	protected override bool UpdateEnabled => MapObjectEntity.IsVisibleForPlayer;

	public ReadOnlyReactiveProperty<string> ObjectSkillCheckText => m_ObjectSkillCheckText;

	public ReadOnlyReactiveProperty<string> ObjectDescription => m_ObjectDescription;

	public ReadOnlyReactiveProperty<string> Name => m_Name;

	public ReadOnlyReactiveProperty<Vector3> CameraDistance => m_CameraDistance;

	public Observable<Unit> CombatObjStateChanged => m_CombatObjStateChanged;

	public ReadOnlyReactiveProperty<AdditionalCombatHintVM> AddCombatInfo => m_AddCombatInfo;

	public ReadOnlyReactiveProperty<int?> HasResourceCount => m_HasResourceCount;

	public ReadOnlyReactiveProperty<bool> CanInteract => m_CanInteract;

	public ReadOnlyReactiveProperty<bool> IsVariative => m_IsVariative;

	public ReadOnlyReactiveProperty<bool> ForceHotKeyPressed => m_ForceHotKeyPressed;

	public ReadOnlyReactiveProperty<bool> ForceHideInCombat => m_ForceHideInCombat;

	public ReadOnlyReactiveProperty<OvertipState> CurrentState => m_CurrentState;

	public ReadOnlyReactiveProperty<bool> IsBarkActive => BarkBlockVM.IsBarkActive;

	public bool IsInCombat => Game.Instance.Player.IsInCombat;

	public UIInteractionType Type => FirstInteractionPart?.UIInteractionType ?? UIInteractionType.None;

	public OvertipMapObjectVM(MapObjectEntity mapObjectEntity)
		: base(mapObjectEntity)
	{
		IEnumerable<AbstractInteractionPart> source = mapObjectEntity.Parts.GetAll<AbstractInteractionPart>();
		FirstInteractionPart = source.FirstOrDefault();
		HasInteractions = FirstInteractionPart != null;
		m_IsVariative.Value = UtilityInteracts.VariativeInteractionCount(MapObjectEntity.View) > 0;
		HasInteractionsWithOvertip = source.Any((AbstractInteractionPart i) => i.ShowOvertip);
		m_ProximityRadius = FirstInteractionPart?.OvertipRevealDistance ?? 6.35f;
		CanBeForceShown = FirstInteractionPart?.CanBeForceShown ?? true;
		BarkBlockVM = new OvertipBarkBlockVM().AddTo(this);
		base.IsEnabled.CombineLatest(base.MapObjectIsHighlighted, base.IsMouseOverUI, (bool isEnabled, bool hover, bool mouseOver) => new { isEnabled, hover, mouseOver }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(_ =>
		{
			m_VisibilityChanged.Execute(Unit.Default);
		})
			.AddTo(this);
		CameraDistance.CombineLatest(IsBarkActive, ForceHotKeyPressed, ForceHideInCombat, (Vector3 _, bool _, bool _, bool _) => new { }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(_ =>
		{
			m_VisibilityChanged.Execute(Unit.Default);
		})
			.AddTo(this);
		m_IsEnabled.Subscribe(delegate
		{
			UpdateAppearanceState();
		}).AddTo(this);
		UpdateObjectData();
		UpdateAppearanceState();
		HighlightChanged();
		GameUIState.Instance.IsInCombat.Subscribe(delegate
		{
			UpdateCombatObjectState();
		}).AddTo(this);
	}

	protected override void OnDispose()
	{
		m_SelectedUnitsSubscription?.Dispose();
	}

	protected override void OnUpdateHandler()
	{
		base.OnUpdateHandler();
		if (MapObjectEntity.IsVisibleForPlayer && HasInteractions)
		{
			m_IsVariative.Value = UtilityInteracts.VariativeInteractionCount(MapObjectEntity.View) > 0;
			UpdateUnitsNear();
			UpdateCanInteract();
			UpdateVariantsCountChanged();
		}
	}

	private void UpdateCombatObjectState()
	{
		if (OvertipUtils.IsAdditionalCombatOvertip(MapObjectEntity) || IsVariative.CurrentValue)
		{
			UpdateAppearanceState();
			UpdateAdditionalCombatObjective();
			m_CombatObjStateChanged?.Execute(Unit.Default);
		}
	}

	private void UpdateAppearanceState()
	{
		if (MapObjectEntity.View != null && FirstInteractionPart != null)
		{
			bool flag = OvertipUtils.IsAdditionalCombatOvertip(MapObjectEntity) && Game.Instance.Player.IsInCombat;
			bool flag2 = OvertipUtils.IsVisited(FirstInteractionPart);
			bool num = MapObjectEntity.IsNewInGame && !FirstInteractionPart.AlreadyVisited;
			bool flag3 = flag && !FirstInteractionPart.AlreadyVisited;
			if (num || flag3 || HasNewVariants)
			{
				m_CurrentState.Value = OvertipState.New;
				HasNewVariants = false;
			}
			else
			{
				m_CurrentState.Value = ((!flag2) ? OvertipState.Unvisited : OvertipState.Default);
			}
		}
	}

	public void HighlightChanged()
	{
		m_MapObjectIsHighlighted.Value = MapObjectEntity != null && MapObjectEntity.View.Highlighted && !MapObjectEntity.View.OnlySilentHighlighting;
	}

	public void ShowBark(string text)
	{
		BarkBlockVM.ShowBark(text);
	}

	public void HideBark()
	{
		BarkBlockVM.HideBark();
	}

	public void Interact()
	{
		if (!FirstInteractionPart || Game.Instance.Controllers.TurnController.IsPreparationTurn)
		{
			return;
		}
		if (UtilityInteracts.VariativeInteractionCount(MapObjectEntity.View) > 0)
		{
			if (FirstInteractionPart.Type == InteractionType.Approach)
			{
				List<BaseUnitEntity> units = FirstInteractionPart.SelectAllUnits(Game.Instance.Controllers.SelectionCharacter.SelectedUnits.ToList()).EmptyIfNull().ToList();
				TryApproachAndInteract(units);
			}
			else if (FirstInteractionPart.Type == InteractionType.Direct)
			{
				EventBus.RaiseEvent(delegate(IVariativeInteractionUIHandler h)
				{
					h.HandleInteractionRequest(MapObjectEntity);
				});
			}
			else
			{
				PFLog.UI.Error("Proximity interactions doesn't supported");
			}
		}
		else if (FirstInteractionPart.Type == InteractionType.Approach)
		{
			List<BaseUnitEntity> units2 = FirstInteractionPart.SelectAllUnits(Game.Instance.Controllers.SelectionCharacter.SelectedUnits.ToList()).EmptyIfNull().ToList();
			TryApproachAndInteract(units2);
		}
		else if (FirstInteractionPart.Type == InteractionType.Direct)
		{
			BaseUnitEntity baseUnitEntity = FirstInteractionPart.SelectUnit(Game.Instance.Controllers.SelectionCharacter.SelectedUnits.ToList());
			ClickMapObjectHandler.ShowWarningIfNeeded(baseUnitEntity, FirstInteractionPart);
			if (baseUnitEntity != null && FirstInteractionPart.CanInteract())
			{
				UnitCommandsRunner.DirectInteract(baseUnitEntity, FirstInteractionPart);
			}
		}
		else
		{
			PFLog.UI.Error("Proximity interactions doesn't supported");
		}
	}

	private void TryApproachAndInteract(List<BaseUnitEntity> units)
	{
		m_CurrentlyApproachingUnits.Clear();
		m_TryApproachingUnits.Clear();
		if (Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			BaseUnitEntity baseUnitEntity = units.FirstItem();
			if (baseUnitEntity != null)
			{
				FirstInteractionPart.SelectUnit(new List<BaseUnitEntity> { baseUnitEntity });
				if (!FirstInteractionPart.IsEnoughCloseForInteractionFromDesiredPosition(baseUnitEntity))
				{
					ClickMapObjectHandler.ShowWarningIfNeeded(baseUnitEntity, FirstInteractionPart);
				}
				else
				{
					UnitCommandsRunner.TryApproachAndInteract(baseUnitEntity, FirstInteractionPart);
				}
			}
			return;
		}
		foreach (BaseUnitEntity approachingUser in units)
		{
			if (approachingUser == null)
			{
				break;
			}
			m_TryApproachingUnits.Add(approachingUser);
			PathfindingService.Instance.FindPathRT(approachingUser.MovementAgent, FirstInteractionPart.Owner.Position, FirstInteractionPart.ApproachRadius.Cells().Meters, delegate(ForcedPath path)
			{
				if (path.error)
				{
					PFLog.Pathfinding.Error("An error path was returned. Ignoring");
					m_TryApproachingUnits.Remove(approachingUser);
				}
				else if (path.vectorPath.Count == 0)
				{
					PFLog.Pathfinding.Error("VectorPath is empty. Ignoring");
					m_TryApproachingUnits.Remove(approachingUser);
				}
				else if (approachingUser.IsMovementLockedByGameModeOrCombat())
				{
					PFLog.Pathfinding.Log("Movement is locked due to GameMode or Combat. Ignoring");
					m_TryApproachingUnits.Remove(approachingUser);
				}
				else
				{
					m_CurrentlyApproachingUnits.Add(approachingUser, path);
					TryApproachAndInteract();
				}
			}, new InteractionCustomDistanceCheck(approachingUser, FirstInteractionPart));
		}
	}

	private void TryApproachAndInteract()
	{
		ForcedPath value;
		foreach (BaseUnitEntity tryApproachingUnit in m_TryApproachingUnits)
		{
			if (!m_CurrentlyApproachingUnits.TryGetValue(tryApproachingUnit, out value))
			{
				return;
			}
		}
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		foreach (KeyValuePair<BaseUnitEntity, ForcedPath> currentlyApproachingUnit in m_CurrentlyApproachingUnits)
		{
			currentlyApproachingUnit.Deconstruct(out var key, out value);
			BaseUnitEntity baseUnitEntity = key;
			ForcedPath forcedPath = value;
			if (forcedPath.vectorPath.Count > 0)
			{
				AbstractInteractionPart firstInteractionPart = FirstInteractionPart;
				List<Vector3> vectorPath = forcedPath.vectorPath;
				if (firstInteractionPart.IsEnoughCloseForInteraction(baseUnitEntity, vectorPath[vectorPath.Count - 1]))
				{
					list.Add(baseUnitEntity);
				}
			}
		}
		BaseUnitEntity unit = ((!list.Empty()) ? FirstInteractionPart.SelectUnit(list) : FirstInteractionPart.SelectUnit(Game.Instance.Controllers.SelectionCharacter.SelectedUnits.ToList()));
		m_CurrentlyApproachingUnits.Clear();
		m_TryApproachingUnits.Clear();
		ClickMapObjectHandler.ShowWarningIfNeeded(unit, FirstInteractionPart);
		UnitCommandsRunner.TryApproachAndInteract(unit, FirstInteractionPart);
	}

	public void UpdateObjectData()
	{
		m_SelectedUnitsSubscription?.Dispose();
		if (MapObjectEntity.View == null || FirstInteractionPart == null)
		{
			return;
		}
		OvertipVerticalCorrection = FirstInteractionPart.OvertipVerticalCorrection;
		bool value = FirstInteractionPart.Enabled;
		AbstractInteractionPart firstInteractionPart = FirstInteractionPart;
		InteractionSkillCheckPart interactionSkillCheckPart = firstInteractionPart as InteractionSkillCheckPart;
		if (interactionSkillCheckPart == null)
		{
			DisableTrapInteractionPart disableTrapInteractionPart = firstInteractionPart as DisableTrapInteractionPart;
			if (disableTrapInteractionPart == null)
			{
				if (!(firstInteractionPart is InteractionDoorPart interactionDoorPart))
				{
					if (!(firstInteractionPart is InteractionLootPart interactionLootPart))
					{
						if (!(firstInteractionPart is InteractionStairsPart))
						{
							if (!(firstInteractionPart is InteractionActionPart interactionActionPart))
							{
								if (firstInteractionPart is InteractionVariativePart interactionVariativePart)
								{
									m_Name.Value = interactionVariativePart.InteractionSettings.DisplayName?.Text;
									m_ObjectSkillCheckText.Value = interactionVariativePart.GetSelectedVariantActor()?.GetInteractionName();
								}
							}
							else
							{
								m_Name.Value = GetString(interactionActionPart.Settings.DisplayName, string.Empty);
							}
						}
						else
						{
							m_Name.Value = ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.Tooltips.Ladder;
							value = Game.Instance.IsControllerGamepad;
						}
					}
					else
					{
						m_Name.Value = interactionLootPart.Name;
					}
				}
				else
				{
					m_Name.Value = ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.Tooltips.Door;
					m_ObjectDescription.Value = (interactionDoorPart.GetState() ? ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.Tooltips.DoorClose : ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.Tooltips.DoorOpen);
					m_ObjectSkillCheckText.Value = string.Empty;
				}
			}
			else
			{
				m_Name.Value = ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.Tooltips.Trap;
				if (disableTrapInteractionPart.Owner.TrapActive)
				{
					m_ObjectDescription.Value = ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.Tooltips.TrapNeutralize;
					SelectionCharacterController selectionCharacter = Game.Instance.Controllers.SelectionCharacter;
					m_ObjectSkillCheckText.Value = UIUtilityText.GetTrapSkillCheckText(disableTrapInteractionPart, selectionCharacter.SelectedUnits.ToList());
					m_SelectedUnitsSubscription = OwlcatR3UnitExtensions.Subscribe(selectionCharacter.ActualGroupUpdated, delegate
					{
						m_ObjectSkillCheckText.Value = UIUtilityText.GetTrapSkillCheckText(disableTrapInteractionPart, selectionCharacter.SelectedUnits.ToList());
					});
				}
				else
				{
					m_ObjectDescription.Value = string.Empty;
					m_ObjectSkillCheckText.Value = string.Empty;
				}
			}
		}
		else
		{
			InteractionSkillCheckSettings settings = interactionSkillCheckPart.Settings;
			if (settings.Type != InteractionType.Variant)
			{
				if (interactionSkillCheckPart.AlreadyUsed && settings.OnlyCheckOnce)
				{
					m_ObjectDescription.Value = (interactionSkillCheckPart.CheckPassed ? GetString(settings.ShortDescriptionPassed, string.Empty) : GetString(settings.ShortDescriptionFailed, string.Empty));
					m_ObjectSkillCheckText.Value = string.Empty;
					m_Name.Value = GetString(settings.DisplayNameAfterUse, string.Empty);
				}
				else
				{
					m_Name.Value = GetString(settings.DisplayName, string.Empty);
					m_ObjectDescription.Value = GetString(settings.ShortDescription, string.Empty);
					m_ObjectSkillCheckText.Value = UtilitySkillcheck.GetOvertipSkillCheckText(interactionSkillCheckPart, Game.Instance.Controllers.SelectionCharacter.SelectedUnits.ToList(), out var needChance);
					if (needChance)
					{
						m_SelectedUnitsSubscription = OwlcatR3UnitExtensions.Subscribe(Game.Instance.Controllers.SelectionCharacter.ActualGroupUpdated, delegate
						{
							m_ObjectSkillCheckText.Value = UtilitySkillcheck.GetOvertipSkillCheckText(interactionSkillCheckPart, Game.Instance.Controllers.SelectionCharacter.SelectedUnits.ToList(), out needChance);
						});
					}
				}
				IInteractionVariantActor interactionVariantActor = MapObjectEntity.GetAll<IInteractionVariantActor>().FirstOrDefault();
				BlueprintItem requiredItem = interactionVariantActor?.RequiredItem;
				if (requiredItem != null)
				{
					RequiredResourceCount = interactionVariantActor.RequiredItemsCount;
					UpdateResourceCount(requiredItem);
					ObservableSubscribeExtensions.Subscribe(m_InventoryChanged, delegate
					{
						UpdateResourceCount(requiredItem);
					}).AddTo(this);
					ResourceName = GetItemName(requiredItem);
				}
			}
		}
		UpdateAdditionalCombatObjective();
		UpdateCanInteract();
		UpdateAppearanceState();
		m_IsEnabled.Value = value;
		void UpdateResourceCount(BlueprintItem blueprintItem)
		{
			m_HasResourceCount.Value = Game.Instance.PartySharedInventory.Collection.Items.Where((ItemEntity i) => i.Blueprint == blueprintItem).Sum((ItemEntity i) => i.Count);
			if (RequiredResourceCount.HasValue)
			{
				m_CanInteract.Value = HasResourceCount.CurrentValue >= RequiredResourceCount;
			}
		}
	}

	public void TriggerInventoryChanged()
	{
		m_InventoryChanged?.Execute(Unit.Default);
	}

	private static string GetItemName(BlueprintItem item)
	{
		if (item == ConfigRoot.Instance.Consumables.MultikeyItem)
		{
			return item.Name;
		}
		if (item == ConfigRoot.Instance.Consumables.MeltaChargeItem)
		{
			return item.Name;
		}
		if (item == ConfigRoot.Instance.Consumables.RitualSetItem)
		{
			return item.Name;
		}
		return UIStrings.Instance.Overtips.NeedUnknownKey.Text;
	}

	private void UpdateAdditionalCombatObjective()
	{
		m_AddCombatInfo.Value?.Dispose();
		m_AddCombatInfo.Value = null;
		if (FirstInteractionPart is InteractionVariativePart interactionVariativePart)
		{
			List<BlueprintAdditionalCombatObjective> list = (from iwc in interactionVariativePart.GetAvailableInteractions()
				where iwc.GetVariantActor().CombatObjective?.Description != null
				select iwc.GetVariantActor().CombatObjective).ToList();
			if (list.Any())
			{
				m_AddCombatInfo.Value = new AdditionalCombatHintVM(list, m_CurrentState).AddTo(this);
			}
		}
		else if (FirstInteractionPart is InteractionPart { Settings: var settings })
		{
			m_HasAdditionalCombatObjective.Value = settings.ShowAdditionalCombatObjective && settings.AdditionalCombatObjective?.Description != null;
			if (m_HasAdditionalCombatObjective.Value)
			{
				m_AddCombatInfo.Value = new AdditionalCombatHintVM(new List<BlueprintAdditionalCombatObjective> { settings.AdditionalCombatObjective }, m_CurrentState).AddTo(this);
			}
		}
	}

	private void UpdateCanInteract()
	{
		NotAvailable = HasInteractionsWithOvertip && !FirstInteractionPart.CanInteract();
		m_CanInteract.Value = ClickMapObjectHandler.HasAvailableInteractions(MapObjectEntity.View.GO) && (!RequiredResourceCount.HasValue || HasResourceCount.CurrentValue >= RequiredResourceCount);
	}

	private void UpdateVariantsCountChanged()
	{
		if ((!MapObjectEntity.IsNewInGame && !GameUIState.Instance.IsInCombat.Value) || !(FirstInteractionPart is InteractionVariativePart interactionVariativePart))
		{
			return;
		}
		int count = interactionVariativePart.PassedConditions.Count;
		int num = interactionVariativePart.InteractionSettings.Interactions.Count(delegate(InteractionWithConditions iwc)
		{
			IInteractionVariantActor variantActor = iwc.GetVariantActor();
			return variantActor != null && variantActor.CanUse && (iwc.ShowConditions == null || iwc.ShowConditions.Count == 0 || iwc.ShowConditions.Any((InteractionWithConditions.ShowReason r) => r.Conditions.IsEmpty() || r.Conditions.Get().Check()));
		});
		if (num != count)
		{
			if (num > count)
			{
				HasNewVariants = true;
			}
			UpdateCombatObjectState();
		}
	}

	public void UpdateInteraction(bool active)
	{
		if (!active)
		{
			UpdateAppearanceState();
		}
	}

	private void UpdateUnitsNear()
	{
		ActiveCharacterIsNear = false;
		MapObjectEntity mapObjectEntity = MapObjectEntity;
		if (mapObjectEntity == null || !mapObjectEntity.IsInCameraFrustum)
		{
			return;
		}
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if ((MapObjectEntity.Position - item.Position).sqrMagnitude <= m_ProximityRadius * m_ProximityRadius)
			{
				ActiveCharacterIsNear = true;
				break;
			}
		}
		if (MapObjectEntity.IsVisibleForPlayer)
		{
			m_CameraDistance.Value = base.Position - CameraRig.Instance.GetTargetPointPosition();
		}
	}

	private string GetString(LocalizedString stringAsset, string def = null)
	{
		if (stringAsset == null)
		{
			return def;
		}
		if (!string.IsNullOrWhiteSpace(stringAsset))
		{
			return stringAsset;
		}
		return def;
	}

	protected override Vector3 GetEntityPosition()
	{
		if (MapObjectEntity?.View == null)
		{
			return Vector3.zero;
		}
		return MapObjectEntity.Position;
	}

	public void HandleHighlightChange(bool isOn)
	{
		m_ForceHotKeyPressed.Value = isOn;
	}

	public void HandleCombatStateChanged()
	{
		if (HasInteractions)
		{
			m_ForceHideInCombat.Value = TurnController.IsInTurnBasedCombat() && FirstInteractionPart.NotInCombat;
			UpdateCanInteract();
		}
	}
}
