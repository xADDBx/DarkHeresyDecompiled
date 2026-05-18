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

	public void Play(UISound type, bool isButton = false, bool playAnyway = false)
	{
		if ((!ShouldNotToPlaySound || isButton || playAnyway) && UIDollRooms.Instance != null)
		{
			Play(type, UIDollRooms.Instance.gameObject, isButton, playAnyway);
		}
	}

	public void Play(UISound sound, GameObject gameObject, bool isButton = false, bool playAnyway = false)
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
			ButtonsSounds buttonsSounds = Instance.Sounds.ButtonsSounds;
			switch (soundType)
			{
			case -1:
				Play(buttonsSounds.Default.Hover, isButton: true);
				break;
			case 0:
				Play(buttonsSounds.PlasticButton.Hover, isButton: true);
				break;
			case 1:
				Play(buttonsSounds.ExitToWarpButton.Hover, isButton: true);
				break;
			case 3:
				Play(buttonsSounds.FinishChargenButton.Hover, isButton: true);
				break;
			case 4:
				Play(buttonsSounds.LootCollectAllButton.Hover, isButton: true);
				break;
			case 5:
				Play(buttonsSounds.DoctrineNextButton.Hover, isButton: true);
				break;
			case 6:
				Play(buttonsSounds.PaperComponentSound.Hover, isButton: true);
				break;
			case 7:
				Play(buttonsSounds.AnalogButton.Hover, isButton: true);
				break;
			case 8:
				Play(buttonsSounds.PaperButton.Hover, isButton: true);
				break;
			case 9:
				Play(buttonsSounds.MajorPaperButton.Hover, isButton: true);
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
		ButtonsSounds buttonsSounds = Instance.Sounds.ButtonsSounds;
		switch (soundType)
		{
		case -1:
			Play(buttonsSounds.Default.Click, isButton: true);
			break;
		case 0:
			Play(buttonsSounds.Default.Click, isButton: true);
			break;
		case 1:
			Play(buttonsSounds.ExitToWarpButton.Click, isButton: true);
			break;
		case 3:
			Play(buttonsSounds.FinishChargenButton.Click, isButton: true);
			break;
		case 4:
			Play(buttonsSounds.LootCollectAllButton.Click, isButton: true);
			break;
		case 5:
			Play(buttonsSounds.DoctrineNextButton.Click, isButton: true);
			break;
		case 6:
			Play(buttonsSounds.PaperComponentSound.Click, isButton: true);
			break;
		case 7:
			Play(buttonsSounds.AnalogButton.Click, isButton: true);
			break;
		case 8:
			Play(buttonsSounds.PaperButton.Click, isButton: true);
			break;
		case 9:
			Play(buttonsSounds.MajorPaperButton.Click, isButton: true);
			break;
		default:
			LogChannel.Default.Warning("UI sound events in OwlcatButton don't supported in project");
			break;
		case -2:
			break;
		}
	}

	public void PlayInteractionSound(UIInteractionType type, GameObject gameObject, bool isSuccess)
	{
		UIInteractionType type2 = ContextData<InteractionVariantData>.Current?.VariantActor?.UIType ?? type;
		SystemSounds.UIInteractionSounds interactionSounds = Instance.Sounds.SystemSounds.InteractionSounds;
		UISound uISound = Instance.Sounds.DoNothingEvent;
		if (interactionSounds.Interactions != null)
		{
			uISound = interactionSounds.GetInteractionSound(type2, isSuccess) ?? uISound;
		}
		Play(uISound, gameObject, isButton: true);
	}

	public void PlayConsoleHintClickSound()
	{
		Play(SystemSounds.Instance.ConsoleHints.Click, isButton: true);
	}

	public void PlayConsoleHintHoldSoundStart()
	{
		Play(SystemSounds.Instance.ConsoleHints.HoldStart, isButton: true);
	}

	public void PlayConsoleHintHoldSoundStop()
	{
		Play(SystemSounds.Instance.ConsoleHints.HoldStop, isButton: true);
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
		UISound type = (split ? Instance.Sounds.ModalWindowsSounds.Loot.CollectAll : Instance.Sounds.ModalWindowsSounds.Loot.CollectOne);
		Play(type);
		PlayItemSound(SlotAction.Take, item, equipSound: false);
	}

	void ICollectLootHandler.HandleCollectAll(ItemsCollection itemsCollection, ItemsCollection to)
	{
	}

	void ISplitItemHandler.HandleSplitItem()
	{
		Play(Instance.Sounds.ModalWindowsSounds.Loot.CollectAll);
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
			Play(Instance.Sounds.CoopSounds.Pings.MobPing);
		}
		else
		{
			Play(Instance.Sounds.CoopSounds.Pings.MobPing, entity.View.GO);
		}
	}

	public void HandlePingPosition(NetPlayer player, Vector3 position)
	{
	}

	public void HandlePingPositionSound(GameObject gameObject)
	{
		Play(Instance.Sounds.CoopSounds.Pings.GroundPing, gameObject);
	}

	public void HandlePingActionBarAbility(NetPlayer player, string keyName, Entity characterEntityRef, int slotIndex)
	{
		Play(Instance.Sounds.CoopSounds.Pings.ActionBarAbilityPing);
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
