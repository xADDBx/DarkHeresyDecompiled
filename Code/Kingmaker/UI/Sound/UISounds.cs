using System;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Items;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Sound.Base;
using Kingmaker.UI.Common;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility.Locator;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.UI.Sound;

public class UISounds : IUIKitSoundManager, IService, IDropItemHandler, ISubscriber, ICollectLootHandler, ISplitItemHandler, IEquipItemAutomaticallyHandler, IDisposable, INetPingEntity, INetPingPosition, INetPingActionBarAbility
{
	public static UISounds Instance => Services.GetInstance<UISounds>();

	public BlueprintUISound Sounds => ConfigRoot.Instance.UIConfig.BlueprintUISound;

	private bool ShouldNotToPlaySound => LoadingProcess.Instance.IsLoadingScreenActive;

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	public UISounds()
	{
		EventBus.Subscribe(this);
	}

	void IDisposable.Dispose()
	{
		EventBus.Unsubscribe(this);
	}

	public void Play(BlueprintUISound.UISound type, bool isButton = false, bool playAnyway = false)
	{
		if ((!ShouldNotToPlaySound || isButton || playAnyway) && UIDollRooms.Instance != null)
		{
			Play(type, UIDollRooms.Instance.gameObject, isButton, playAnyway);
		}
	}

	public void Play(BlueprintUISound.UISound sound, GameObject gameObject, bool isButton = false, bool playAnyway = false)
	{
		if (sound != null && !string.IsNullOrEmpty(sound.Id) && (!ShouldNotToPlaySound || isButton || playAnyway))
		{
			SoundEventsManager.PostEvent(sound.Id, gameObject);
		}
	}

	public void PlayHoverSound(int soundType = -1)
	{
		if (!GameUIState.Instance.IsInventoryDollRotating)
		{
			switch (soundType)
			{
			case -1:
				Play(Instance.Sounds.Buttons.ButtonHover, isButton: true);
				break;
			case 0:
				Play(Instance.Sounds.Buttons.PlastickButtonHover, isButton: true);
				break;
			case 1:
				Play(Instance.Sounds.Buttons.ExitToWarpButtonHover, isButton: true);
				break;
			case 2:
				Play(Instance.Sounds.SpaceColonization.ProjectWindowButtonHover, isButton: true);
				break;
			case 3:
				Play(Instance.Sounds.Buttons.FinishChargenButtonHover, isButton: true);
				break;
			case 4:
				Play(Instance.Sounds.Buttons.LootCollectAllButtonHover, isButton: true);
				break;
			case 5:
				Play(Instance.Sounds.Buttons.DoctrineNextButtonHover, isButton: true);
				break;
			case 6:
				Play(Instance.Sounds.Buttons.PaperComponentSoundHover, isButton: true);
				break;
			case 7:
				Play(Instance.Sounds.Buttons.AnalogButtonHover, isButton: true);
				break;
			case 8:
				Play(Instance.Sounds.Buttons.PaperButtonHover, isButton: true);
				break;
			case 9:
				Play(Instance.Sounds.Buttons.MajorPaperButtonHover, isButton: true);
				break;
			default:
				LogChannel.Default.Warning("UI sound events in OwlcatButton don't supported in project");
				break;
			case -2:
				break;
			}
		}
	}

	public void PlayButtonClickSound(int soundType = -1)
	{
		switch (soundType)
		{
		case -1:
			Play(Instance.Sounds.Buttons.ButtonClick, isButton: true);
			break;
		case 0:
			Play(Instance.Sounds.Buttons.PlastickButtonClick, isButton: true);
			break;
		case 1:
			Play(Instance.Sounds.Buttons.ExitToWarpButtonClick, isButton: true);
			break;
		case 3:
			Play(Instance.Sounds.Buttons.FinishChargenButtonClick, isButton: true);
			break;
		case 4:
			Play(Instance.Sounds.Buttons.LootCollectAllButtonClick, isButton: true);
			break;
		case 5:
			Play(Instance.Sounds.Buttons.DoctrineNextButtonClick, isButton: true);
			break;
		case 6:
			Play(Instance.Sounds.Buttons.PaperComponentSoundClick, isButton: true);
			break;
		case 7:
			Play(Instance.Sounds.Buttons.AnalogButtonClick, isButton: true);
			break;
		case 8:
			Play(Instance.Sounds.Buttons.PaperButtonClick, isButton: true);
			break;
		case 9:
			Play(Instance.Sounds.Buttons.MajorPaperButtonClick, isButton: true);
			break;
		default:
			LogChannel.Default.Warning("UI sound events in OwlcatButton don't supported in project");
			break;
		case -2:
		case 2:
			break;
		}
	}

	public void PlayInteractionSound(UIInteractionType type, GameObject gameObject, bool isSuccess)
	{
		UIInteractionType type2 = ContextData<InteractionVariantData>.Current?.VariantActor?.UIType ?? type;
		BlueprintUISound sounds = Instance.Sounds;
		BlueprintUISound.UISound sound = sounds.DoNothingEvent;
		if (sounds.InteractionSounds.Interactions != null)
		{
			sound = sounds.InteractionSounds.GetInteractionSound(type2, isSuccess) ?? sounds.DoNothingEvent;
		}
		Play(sound, gameObject, isButton: true);
	}

	public void PlayConsoleHintClickSound()
	{
		Play(Instance.Sounds.ConsoleHints.ConsoleHintClick, isButton: true);
	}

	public void PlayConsoleHintHoldSoundStart()
	{
		Play(Instance.Sounds.ConsoleHints.ConsoleHintHoldStart, isButton: true);
	}

	public void PlayConsoleHintHoldSoundStop()
	{
		Play(Instance.Sounds.ConsoleHints.ConsoleHintHoldStop, isButton: true);
	}

	public void PlayConsoleHintHoldSoundSetRtpcValue(float value)
	{
		AkUnitySoundEngine.SetRTPCValue("UI_HintPitch", value);
	}

	public void PlayItemSound(SlotAction action, ItemEntity item, bool equipSound)
	{
		if (item == null)
		{
			PFLog.UI.Error("Item for PlayItemSound cannot be null");
			return;
		}
		string text = string.Empty;
		if (equipSound)
		{
			if (item.Blueprint is BlueprintItemEquipment blueprintItemEquipment)
			{
				switch (action)
				{
				case SlotAction.Put:
					text = blueprintItemEquipment.InventoryEquipSound;
					break;
				case SlotAction.Take:
					text = blueprintItemEquipment.InventoryPutSound;
					break;
				}
			}
		}
		else
		{
			switch (action)
			{
			case SlotAction.Put:
				text = item.Blueprint.InventoryPutSound;
				break;
			case SlotAction.Take:
				text = item.Blueprint.InventoryTakeSound;
				break;
			}
		}
		if (!string.IsNullOrEmpty(text) && !ShouldNotToPlaySound)
		{
			SoundEventsManager.PostEvent(text, UIDollRooms.Instance.gameObject);
		}
	}

	void IEquipItemAutomaticallyHandler.HandleEquipItemAutomatically(ItemEntity item)
	{
	}

	void IDropItemHandler.HandleDropItem(ItemEntity item, bool split)
	{
		Play(split ? Instance.Sounds.Loot.LootCollectAll : Instance.Sounds.Loot.LootCollectOne);
		PlayItemSound(SlotAction.Take, item, equipSound: false);
	}

	void ICollectLootHandler.HandleCollectAll(ItemsCollection itemsCollection, ItemsCollection to)
	{
	}

	void ISplitItemHandler.HandleSplitItem()
	{
		Play(Instance.Sounds.Loot.LootCollectAll);
	}

	void ISplitItemHandler.HandleAfterSplitItem(ItemEntity item)
	{
	}

	void ISplitItemHandler.HandleBeforeSplitItem(ItemEntity item, ItemsCollection itemsCollection, ItemsCollection to)
	{
	}

	public void HandlePingEntity(NetPlayer player, Entity entity)
	{
		if (entity?.View == null)
		{
			Play(Instance.Sounds.Coop.MobPing);
		}
		else
		{
			Play(Instance.Sounds.Coop.MobPing, entity.View.GO);
		}
	}

	public void HandlePingPosition(NetPlayer player, Vector3 position)
	{
	}

	public void HandlePingPositionSound(GameObject gameObject)
	{
		Play(Instance.Sounds.Coop.GroundPing, gameObject);
	}

	public void HandlePingActionBarAbility(NetPlayer player, string keyName, Entity characterEntityRef, int slotIndex)
	{
		Play(Instance.Sounds.Coop.ActionBarAbilityPing);
	}

	public void SetClickAndHoverSound(OwlcatSelectable soundObject, ButtonSoundsEnum soundType)
	{
		if (!(soundObject == null))
		{
			SetClickSound(soundObject, soundType);
			SetHoverSound(soundObject, soundType);
		}
	}

	public void SetClickSound(OwlcatSelectable soundObject, ButtonSoundsEnum soundType)
	{
		if (!(soundObject == null))
		{
			soundObject.ClickSoundType = (int)soundType;
		}
	}

	public void SetHoverSound(OwlcatSelectable soundObject, ButtonSoundsEnum soundType)
	{
		if (!(soundObject == null))
		{
			soundObject.HoverSoundType = (int)soundType;
		}
	}
}
