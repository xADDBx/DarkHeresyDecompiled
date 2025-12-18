using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Designers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Settings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class DialogNotificationsVM : ViewModel, IItemsCollectionHandler, ISubscriber, IDialogNotificationHandler, IDialogCueHandler, IBookPageHandler, IDamageHandler, IConvictionShiftHandler, IGainFactionReputationHandler, IEntityGainFactHandler, ISubscriber<IMechanicEntity>, ICaseStatusChanged, IClueStatusChanged, IClueAddendumStatusChanged, IConclusionStatusChanged
{
	public class FactionReputation
	{
		private class FactionData
		{
			public FactionType FactionType { get; }

			public ReputationType ReputationType { get; }

			public int Count { get; private set; }

			public FactionData(FactionType factionType, ReputationType reputationType, int count)
			{
				FactionType = factionType;
				ReputationType = reputationType;
				Count = count;
			}

			public void AddCount(int value)
			{
				Count += value;
			}
		}

		private List<FactionData> Factions { get; } = new List<FactionData>();


		public bool Any => Factions.Any();

		public void Add(FactionType factionType, ReputationType reputationType, int count)
		{
			FactionData factionData = Factions.FirstOrDefault((FactionData f) => f.FactionType == factionType && f.ReputationType == reputationType);
			if (factionData != null)
			{
				factionData.AddCount(count);
			}
			else
			{
				Factions.Add(new FactionData(factionType, reputationType, count));
			}
		}

		public List<(FactionType, ReputationType, int)> GetData()
		{
			return Factions.Select((FactionData f) => (FactionType: f.FactionType, ReputationType: f.ReputationType, Count: f.Count)).ToList();
		}

		public List<(FactionType, ReputationType, int)> GetLostData()
		{
			return (from f in Factions
				where f.Count < 0
				select (FactionType: f.FactionType, ReputationType: f.ReputationType, Count: f.Count)).ToList();
		}

		public List<(FactionType, ReputationType, int)> GetReceivedData()
		{
			return (from f in Factions
				where f.Count > 0
				select (FactionType: f.FactionType, ReputationType: f.ReputationType, Count: f.Count)).ToList();
		}

		public void Clear()
		{
			Factions.Clear();
		}
	}

	public readonly List<string> RevealedLocationNames = new List<string>();

	public readonly Dictionary<ItemEntity, int> ItemsReceived = new Dictionary<ItemEntity, int>();

	public readonly Dictionary<ItemEntity, int> ItemsLost = new Dictionary<ItemEntity, int>();

	public readonly List<EntityFact> AbilityAdded = new List<EntityFact>();

	public readonly List<EntityFact> BuffAdded = new List<EntityFact>();

	public readonly Dictionary<string, int> DamageDealt = new Dictionary<string, int>();

	public readonly FactionReputation FactionReputationChanged = new FactionReputation();

	public readonly List<string> CustomNotifications = new List<string>();

	public readonly Dictionary<AlignmentAxis, int> AlignmentShifts = new Dictionary<AlignmentAxis, int>();

	private readonly ReactiveCommand<bool> m_HasNotifications = new ReactiveCommand<bool>();

	public readonly List<BlueprintCase> CasesOpened = new List<BlueprintCase>();

	public readonly List<BlueprintCase> CasesClosed = new List<BlueprintCase>();

	public readonly List<BlueprintClue> CluesReceived = new List<BlueprintClue>();

	public readonly List<BlueprintClueAddendum> AddendumsReceived = new List<BlueprintClueAddendum>();

	public readonly List<BlueprintConclusion> ConclusionsConstructed = new List<BlueprintConclusion>();

	public Observable<bool> HasNotifications => m_HasNotifications;

	public DialogNotificationsVM()
	{
		EventBus.Subscribe(this).AddTo(this);
	}

	private void OnUpdate()
	{
		bool parameter = RevealedLocationNames.Count > 0 || ItemsLost.Count > 0 || ItemsReceived.Count > 0 || CustomNotifications.Count > 0 || DamageDealt.Count > 0 || AlignmentShifts.Count > 0 || FactionReputationChanged.Any || AbilityAdded.Count > 0 || BuffAdded.Count > 0 || CasesOpened.Any() || CasesClosed.Any() || CluesReceived.Any() || AddendumsReceived.Any() || ConclusionsConstructed.Any();
		m_HasNotifications.Execute(parameter);
		Clear();
	}

	private void Clear()
	{
		RevealedLocationNames.Clear();
		ItemsReceived.Clear();
		ItemsLost.Clear();
		CustomNotifications.Clear();
		DamageDealt.Clear();
		AlignmentShifts.Clear();
		FactionReputationChanged.Clear();
		AbilityAdded.Clear();
		BuffAdded.Clear();
		CasesOpened.Clear();
		CasesClosed.Clear();
		CluesReceived.Clear();
		AddendumsReceived.Clear();
		ConclusionsConstructed.Clear();
	}

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if (SettingsRoot.Game.Dialogs.ShowItemsReceivedNotification.GetValue() && collection == GameHelper.GetPlayerCharacter().Inventory.Collection && item?.Blueprint != null && !string.IsNullOrWhiteSpace(item.Name) && count != 0)
		{
			if (ItemsReceived.TryGetValue(item, out var _))
			{
				ItemsReceived[item] += count;
			}
			else
			{
				ItemsReceived.Add(item, count);
			}
		}
	}

	public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
		if (SettingsRoot.Game.Dialogs.ShowItemsReceivedNotification.GetValue() && collection.IsPlayerInventory && item.IsLootable && item.Blueprint != null && !string.IsNullOrWhiteSpace(item.Name) && count != 0)
		{
			if (ItemsLost.TryGetValue(item, out var _))
			{
				ItemsLost[item] += count;
			}
			else
			{
				ItemsLost.Add(item, count);
			}
		}
	}

	public void AddCustomNotification(string text)
	{
		CustomNotifications.Add(text);
	}

	public void HandleOnCueShow(CueShowData cueShowData)
	{
		OnUpdate();
	}

	public void HandleOnBookPageShow(BlueprintBookPage page, List<CueShowData> cues, List<BlueprintAnswer> answers)
	{
		OnUpdate();
	}

	public void HandleDamageDealt(RuleDealDamage dealDamage)
	{
		if (DamageDealt.TryGetValue(dealDamage.ConcreteTarget.Name ?? string.Empty, out var _))
		{
			DamageDealt[dealDamage.ConcreteTarget.Name ?? string.Empty] += dealDamage.ResultValue;
		}
		else
		{
			DamageDealt.Add(dealDamage.ConcreteTarget.Name ?? string.Empty, dealDamage.ResultValue);
		}
	}

	public void HandleAlignmentShift(IAlignmentShiftProvider provider)
	{
		foreach (AlignmentShift alignmentShift in provider.AlignmentShifts)
		{
			if (alignmentShift.NoShift)
			{
				break;
			}
			AlignmentAxis axis = alignmentShift.Axis;
			int value = alignmentShift.Value;
			if (AlignmentShifts.TryGetValue(alignmentShift.Axis, out var _))
			{
				AlignmentShifts[axis] += value;
			}
			else
			{
				AlignmentShifts.Add(axis, value);
			}
		}
	}

	public void HandleGainFactionReputation(FactionType factionType, ReputationType reputationType, int count)
	{
		OnFactionReputationReceived(factionType, reputationType, count);
	}

	private void OnFactionReputationReceived(FactionType factionType, ReputationType reputationType, int count)
	{
		FactionReputationChanged.Add(factionType, reputationType, count);
	}

	public void HandleEntityGainFact(EntityFact fact)
	{
		if (!(fact.Owner is BaseUnitEntity { IsInPlayerParty: not false }))
		{
			return;
		}
		string text = ((!(fact.Owner is BaseUnitEntity baseUnitEntity2)) ? string.Empty : baseUnitEntity2.CharacterName);
		string value = text;
		if ((fact is Buff && fact.Blueprint is BlueprintBuff blueprintBuff && (blueprintBuff.IsHiddenInUI || !blueprintBuff.ShowInDialogue)) || (fact is Ability && fact.Blueprint is BlueprintAbility { ShowInDialogue: false }) || (fact is Feature && fact.Blueprint is BlueprintFeature { ShowInDialogue: false }) || string.IsNullOrWhiteSpace(fact.Name) || string.IsNullOrWhiteSpace(value))
		{
			return;
		}
		if (!(fact is Buff))
		{
			if (!(fact is Ability))
			{
				if (fact is Feature)
				{
					AbilityAdded.Add(fact);
				}
			}
			else
			{
				AbilityAdded.Add(fact);
			}
		}
		else
		{
			BuffAdded.Add(fact);
		}
	}

	public void HandleCaseStatusChanged(BlueprintCase blueprint)
	{
		switch (Game.Instance.DetectiveSystem.GetCaseStatus(blueprint))
		{
		case CaseStatus.Opened:
			CasesOpened.Add(blueprint);
			break;
		case CaseStatus.Closed:
			CasesClosed.Add(blueprint);
			break;
		}
	}

	public void HandleClueStatusChanged(BlueprintClue blueprint)
	{
		if (!blueprint.ParentCase.Blueprint.IsClosed() && Game.Instance.DetectiveSystem.HasClue(blueprint) && !CluesReceived.Any((BlueprintClue c) => c.HasOverride(blueprint)))
		{
			CluesReceived.Add(blueprint);
			AddendumsReceived.RemoveAll((BlueprintClueAddendum a) => a.ParentClue.Blueprint == blueprint);
		}
	}

	public void HandleClueAddendumStatusChanged(BlueprintClueAddendum blueprint)
	{
		if (!blueprint.ParentCase.Blueprint.IsOpen() && Game.Instance.DetectiveSystem.HasClue(blueprint.ParentClue))
		{
			AddendumsReceived.Add(blueprint);
		}
	}

	public void HandleConclusionStatusChanged(BlueprintConclusion blueprint)
	{
		if (!blueprint.ParentCase.Blueprint.IsClosed())
		{
			ConclusionsConstructed.Add(blueprint);
		}
	}
}
