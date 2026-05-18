using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Code.Gameplay.Features.DetectiveClues.View;
using Kingmaker.Code.Gameplay.Features.VariableInteractions;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.Interaction;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionRestrictions;
using Kingmaker.View.Mechanics.Interactions.Restrictions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class VariativeInteractionVM : ViewModel, ITurnEndHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	private static class InteractionPriority
	{
		public static int GetPriority(IInteractionVariantActor actor)
		{
			if (!(actor is InteractionSkillCheckPart) && !(actor is InteractionActionPart))
			{
				if (!(actor is SleightOfHandRestrictionPart) && !(actor is SkillUseWithoutToolRestrictionPart) && !(actor is LoreXenosRestrictionPart))
				{
					if (!(actor is UnlockRestrictionPart) && !(actor is LoreXenosMultikeyItemRestrictionPart) && !(actor is SleightOfHandMultikeyItemRestrictionPart) && !(actor is KeyRestrictionPart))
					{
						if (!(actor is DemolitionMeltaChargeRestrictionPart) && !(actor is MeltaChargeRestrictionPart) && !(actor is MultikeyRestrictionPart) && !(actor is RitualSetRestrictionPart))
						{
							if (actor is InteractionPartDetectiveClue || actor is InteractionPartDetectiveTrace)
							{
								return 50;
							}
							PFLog.UI.Error("Unsupported priority for " + actor.GetType().Name + " actor");
							return 100;
						}
						return 30;
					}
					return 20;
				}
				return 10;
			}
			return 0;
		}
	}

	public readonly MechanicEntity MechanicEntity;

	public readonly SelectionGroupRadioVM<InteractionVariantVM> Variants;

	public readonly InteractionVariativePart InteractionPart;

	private readonly ReactiveProperty<InteractionVariantVM> m_SelectedVariant = new ReactiveProperty<InteractionVariantVM>();

	private readonly Action m_CloseCallback;

	public string Title => InteractionPart?.InteractionSettings.DisplayName?.Text;

	public VariativeType VariativeType => InteractionPart?.InteractionSettings.VariativeType ?? VariativeType.Default;

	public bool IsToggleGroup => VariativeType == VariativeType.ToggleGroup;

	public bool HasChance => Variants.EntitiesCollection.Any((InteractionVariantVM v) => !string.IsNullOrEmpty(v.InteractionChance.CurrentValue));

	public Vector3 ObjectWorldPosition
	{
		get
		{
			if (!CheckMapObject())
			{
				return Vector3.zero;
			}
			return MechanicEntity.ViewPosition;
		}
	}

	private bool CheckMapObject()
	{
		if (MechanicEntity != null)
		{
			return true;
		}
		m_CloseCallback?.Invoke();
		return false;
	}

	public VariativeInteractionVM(MechanicEntity mechanicEntity, IEnumerable<InteractionActorWithConditions> actorData, Action closeCallback)
	{
		MechanicEntity = mechanicEntity;
		m_CloseCallback = closeCallback;
		InteractionPart = MechanicEntity.Parts.GetOptional<InteractionVariativePart>();
		InteractionPart?.SetVisited();
		if (actorData == null)
		{
			actorData = InteractionPart?.InteractionSettings.Interactions?.Select((InteractionWithConditions v) => v.ToActorWithConditions()) ?? (from a in UtilityInteracts.GetIHasInteractionVariants(MechanicEntity).GetInteractionVariantActors()
				select new InteractionActorWithConditions(a));
		}
		actorData = actorData.OrderBy((InteractionActorWithConditions c) => InteractionPriority.GetPriority(c.VariantActor));
		List<InteractionVariantVM> list = new List<InteractionVariantVM>();
		foreach (InteractionActorWithConditions actorDatum in actorData)
		{
			IInteractionVariantActor actor = actorDatum.VariantActor;
			if (actor is UnlockRestrictionPart || (actorDatum.ShowReasons.Count > 0 && !actorDatum.ShowReasons.Any((InteractionWithConditions.ShowReason r) => r.Conditions.Get()?.Check() ?? false)) || !actor.CanUse)
			{
				continue;
			}
			BlueprintItem item = actor.RequiredItem;
			int? resourceCount = null;
			int? requiredResourceCount = null;
			ConditionsHolder conditionsHolder = InteractionPart?.PassedConditions?.FirstOrDefault((KeyValuePair<MapObjectEntity, ConditionsHolder> c) => c.Key == actor.InteractionPart.View.Data).Value;
			if (conditionsHolder == null)
			{
				conditionsHolder = actorDatum.ShowReasons.FirstOrDefault((InteractionWithConditions.ShowReason r) => r.Conditions.Get().Check())?.Conditions;
				InteractionPart?.TrySetPassedConditions(conditionsHolder);
			}
			string reasonFor = actorDatum.GetReasonFor(conditionsHolder);
			if (item != null && actor.RequiredItemsCount.HasValue)
			{
				resourceCount = Game.Instance.PartySharedInventory.Collection.Items.Where((ItemEntity i) => i.Blueprint == item).Sum((ItemEntity i) => i.Count);
				requiredResourceCount = actor.RequiredItemsCount;
			}
			InteractionVariantVM item2 = ((!IsToggleGroup) ? new InteractionVariantVM(actorDatum, item?.Name, resourceCount, requiredResourceCount, null, reasonFor, Close).AddTo(this) : new NewInteractionVariantToggleVM(actorDatum, item?.Name, reasonFor, delegate
			{
				InteractionPart.SetSelectedVariant(actor);
				Close();
			}).AddTo(this));
			list.Add(item2);
		}
		m_SelectedVariant.Value = list.FirstOrDefault((InteractionVariantVM vm) => vm.InteractionActor.VariantActor == InteractionPart?.GetSelectedVariantActor());
		Variants = new SelectionGroupRadioVM<InteractionVariantVM>(list, m_SelectedVariant);
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		Variants?.Dispose();
	}

	public void Close()
	{
		m_CloseCallback?.Invoke();
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		if (Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			Close();
		}
	}
}
