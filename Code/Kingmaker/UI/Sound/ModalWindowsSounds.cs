using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UI.Sound;

[Serializable]
public class ModalWindowsSounds
{
	[Serializable]
	public class UISoundTooltip
	{
		[field: SerializeField]
		public UISound Show { get; private set; }

		[field: SerializeField]
		public UISound Hide { get; private set; }
	}

	[Serializable]
	public class UISoundLoot
	{
		[Serializable]
		public class LootSoundEntry
		{
			[field: SerializeField]
			public LootWindowMode WindowMode { get; private set; }

			[field: SerializeField]
			public UISound SoundWindowOpen { get; private set; }

			[field: SerializeField]
			public UISound SoundWindowClose { get; private set; }
		}

		[field: Header("Open/Close sounds")]
		[field: SerializeField]
		public List<LootSoundEntry> LootSounds { get; private set; } = new List<LootSoundEntry>();


		[field: SerializeField]
		public UISound DefaultWindowOpen { get; private set; }

		[field: SerializeField]
		public UISound DefaultWindowClose { get; private set; }

		[field: Header("Loot sounds")]
		[field: SerializeField]
		public UISound CollectOne { get; private set; }

		[field: SerializeField]
		public UISound CollectAll { get; private set; }

		[field: SerializeField]
		public UISound InsertableLootDrop { get; private set; }

		[field: Header("TrashMode")]
		[field: SerializeField]
		public UISound ActivateTrashMode { get; private set; }

		[field: SerializeField]
		public UISound DeactivateTrashMode { get; private set; }

		[field: SerializeField]
		public UISound MarkAsTrash { get; private set; }

		public UISound GetLootWindowOpenSound(LootWindowMode windowMode)
		{
			LootSoundEntry lootSoundEntry = LootSounds.FirstOrDefault((LootSoundEntry x) => x.WindowMode == windowMode);
			if (lootSoundEntry == null)
			{
				return DefaultWindowOpen;
			}
			return lootSoundEntry.SoundWindowOpen;
		}

		public UISound GetLootWindowCloseSound(LootWindowMode windowMode)
		{
			LootSoundEntry lootSoundEntry = LootSounds.FirstOrDefault((LootSoundEntry x) => x.WindowMode == windowMode);
			if (lootSoundEntry == null)
			{
				return DefaultWindowClose;
			}
			return lootSoundEntry.SoundWindowClose;
		}
	}

	[Serializable]
	public class UISoundTutorial
	{
		[field: SerializeField]
		public UISound ShowBigTutorial { get; private set; }

		[field: SerializeField]
		public UISound HideBigTutorial { get; private set; }

		[field: SerializeField]
		public UISound ShowSmallTutorial { get; private set; }

		[field: SerializeField]
		public UISound HideSmallTutorial { get; private set; }

		[field: SerializeField]
		public UISound ChangeTutorialPage { get; private set; }

		[field: SerializeField]
		public UISound BanTutorialType { get; private set; }
	}

	[Serializable]
	public class UISoundMessageBox
	{
		[field: SerializeField]
		public UISound Show { get; private set; }

		[field: SerializeField]
		public UISound Hide { get; private set; }
	}

	[Serializable]
	public class UISoundGroupChanger
	{
		[field: SerializeField]
		public UISound Open { get; private set; }

		[field: SerializeField]
		[field: FormerlySerializedAs("GroupChangerClose")]
		public UISound Close { get; private set; }
	}

	[Serializable]
	public class UISoundPartySelectorConsole
	{
		[field: SerializeField]
		public UISound SelectOne { get; private set; }

		[field: SerializeField]
		public UISound UnselectOne { get; private set; }

		[field: SerializeField]
		public UISound SelectAll { get; private set; }

		[field: SerializeField]
		public UISound UnselectAll { get; private set; }
	}

	public static ModalWindowsSounds Instance => Services.GetInstance<UISounds>().Sounds.ModalWindowsSounds;

	[field: SerializeField]
	public UISoundTooltip Tooltip { get; private set; }

	[field: SerializeField]
	public UISoundLoot Loot { get; private set; }

	[field: SerializeField]
	public UISoundTutorial Tutorial { get; private set; }

	[field: SerializeField]
	public UISoundMessageBox MessageBox { get; private set; }

	[field: SerializeField]
	public UISoundGroupChanger GroupChanger { get; private set; }

	[field: SerializeField]
	public UISoundPartySelectorConsole PartySelectorConsole { get; private set; }
}
