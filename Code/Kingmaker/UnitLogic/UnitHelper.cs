using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.Visual.Animation;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.AR;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.UnitLogic;

public static class UnitHelper
{
	public class PreviewUnit : ContextFlag<PreviewUnit>
	{
	}

	public class ChargenUnit : ContextFlag<ChargenUnit>
	{
	}

	public class RespecInProgress : ContextFlag<RespecInProgress>
	{
	}

	public class DoNotCreateItems : ContextFlag<DoNotCreateItems>
	{
	}

	public struct DamageEstimate
	{
		public int Value;

		public bool BypassDR;

		public IntermediateDamage[] Chunks;
	}

	private class CloseToFarNodesEnumerator : IDisposable, IEnumerator<GridNodeBase>, IEnumerator
	{
		private class NodeData : IComparable<NodeData>
		{
			public readonly GridNodeBase node;

			public readonly int pathLenght;

			public readonly int diagonalCount;

			public NodeData(GridNodeBase node, int pathLenght, int diagonalCount)
			{
				this.node = node;
				this.pathLenght = pathLenght;
				this.diagonalCount = diagonalCount;
			}

			public int CompareTo(NodeData other)
			{
				if (pathLenght != other.pathLenght)
				{
					return pathLenght - other.pathLenght;
				}
				return diagonalCount - other.diagonalCount;
			}
		}

		private HashSet<GridNodeBase> m_Processed;

		private PriorityQueue<NodeData> m_Queue;

		private Vector3 m_Position;

		private bool m_Started;

		public GridNodeBase Current => m_Queue.Peek().node;

		object IEnumerator.Current => m_Queue.Peek().node;

		public CloseToFarNodesEnumerator(Vector3 position)
		{
			m_Processed = CollectionPool<HashSet<GridNodeBase>, GridNodeBase>.Get();
			m_Queue = CollectionPool<PriorityQueue<NodeData>, NodeData>.Get();
			m_Position = position;
		}

		public void Dispose()
		{
			CollectionPool<HashSet<GridNodeBase>, GridNodeBase>.Release(m_Processed);
			CollectionPool<PriorityQueue<NodeData>, NodeData>.Release(m_Queue);
		}

		public bool MoveNext()
		{
			if (!m_Started)
			{
				m_Started = true;
				GridNodeBase nearestNodeXZ = m_Position.GetNearestNodeXZ();
				if (nearestNodeXZ != null)
				{
					m_Queue.Enqueue(new NodeData(nearestNodeXZ, 0, 0));
				}
			}
			else
			{
				m_Processed.Add(m_Queue.Dequeue().node);
			}
			while (m_Queue.Count > 0 && m_Processed.Contains(m_Queue.Peek().node))
			{
				m_Queue.Dequeue();
			}
			while (m_Queue.Count > 0)
			{
				NodeData nodeData = m_Queue.Peek();
				if (m_Processed.Contains(nodeData.node))
				{
					m_Queue.Dequeue();
					continue;
				}
				GridNodeBase node = nodeData.node;
				for (int i = 0; i < 8; i++)
				{
					GridNodeBase neighbourAlongDirection = node.GetNeighbourAlongDirection(i);
					if (neighbourAlongDirection != null)
					{
						bool flag = i > 3;
						int num = nodeData.diagonalCount + (flag ? 1 : 0);
						int pathLenght = nodeData.pathLenght + 1 + ((flag && num % 2 == 0) ? 1 : 0);
						m_Queue.Enqueue(new NodeData(neighbourAlongDirection, pathLenght, num));
					}
				}
				break;
			}
			return m_Queue.Count > 0;
		}

		public void Reset()
		{
			m_Processed.Clear();
			m_Queue.Clear();
			m_Started = false;
		}
	}

	public enum MoveCommandStatus
	{
		NewCommandCreated,
		SamePath,
		NotEnoughPoints,
		NoReachableTile,
		NoForcedPath,
		NoStartingCell,
		CannotMove
	}

	public static BaseUnitEntity Copy(this BaseUnitEntity unit, bool createView, bool preview, bool copyItems = true)
	{
		try
		{
			using (ProfileScope.New("Copy Unit"))
			{
				return CopyInternal(unit, createView, preview, copyItems);
			}
		}
		catch (Exception exception)
		{
			LogChannel.Default.ExceptionWithReport(exception, null);
			return null;
		}
	}

	private static BaseUnitEntity CopyInternal(BaseUnitEntity unit, bool createView, bool preview, bool copyItems)
	{
		BaseUnitEntity baseUnitEntity;
		using (ContextData<PreviewUnit>.RequestIf(preview))
		{
			using (ContextData<DoNotCreateItems>.Request())
			{
				using (ContextData<AddClassLevels.DoNotCreatePlan>.RequestIf(preview))
				{
					baseUnitEntity = unit.OriginalBlueprint.CreateEntity();
				}
			}
		}
		baseUnitEntity.CopyOf = unit;
		baseUnitEntity.Unsubscribe();
		baseUnitEntity.Description.SetName(unit.Description.CustomName);
		baseUnitEntity.UISettings.SetPortrait(unit.Portrait);
		baseUnitEntity.ViewSettings.SetDoll(unit.ViewSettings.Doll);
		baseUnitEntity.Inventory.EnsureOwn();
		if (preview)
		{
			baseUnitEntity.Facts.EnsureFactProcessor<BuffCollection>().SetupPreview(baseUnitEntity);
		}
		baseUnitEntity.Progression.CopyFrom(unit.Progression);
		CopyFacts(unit, baseUnitEntity);
		if (copyItems)
		{
			CopyItems(unit, baseUnitEntity);
		}
		baseUnitEntity.Progression.FixCharacterLevelAfterCopy();
		if (createView)
		{
			baseUnitEntity.AttachToViewOnLoad(null);
		}
		baseUnitEntity.Subscribe();
		return baseUnitEntity;
	}

	private static void CopyFacts(BaseUnitEntity original, BaseUnitEntity target)
	{
		foreach (EntityFact item in original.Facts.List)
		{
			UnitFact unitFact = item as UnitFact;
			if (item.SourceItem != null)
			{
				continue;
			}
			Feature feature = unitFact as Feature;
			Feature feature2 = ((feature != null) ? target.Progression.Features.Get(feature.Blueprint) : null);
			if ((feature != null && (feature2 == null || feature2.GetRank() < feature.GetRank())) || !target.Facts.Contains(item.Blueprint))
			{
				EntityFact entityFact = feature2 ?? ((unitFact != null) ? ((EntityFact)unitFact.Blueprint.CreateFact(unitFact.MaybeContext, null)) : ((EntityFact)new EntityFactBlueprinted(item.Blueprint)));
				if (entityFact is Feature feature3 && feature != null)
				{
					feature3.SetSamePathSource(feature);
					feature3.Param = feature.Param;
				}
				if (entityFact is IFactWithRanks factWithRanks && unitFact is IFactWithRanks factWithRanks2)
				{
					int count = factWithRanks2.Rank - entityFact.GetRank();
					factWithRanks.AddRank(count);
				}
				if (entityFact != feature2)
				{
					target.Facts.Add(entityFact);
				}
			}
		}
	}

	private static void CopyItems(BaseUnitEntity original, BaseUnitEntity target)
	{
		target.Body.CurrentHandEquipmentSetIndex = original.Body.CurrentHandEquipmentSetIndex;
		List<ItemSlot> list = original.Body.EquipmentSlots.ToTempList();
		List<ItemSlot> list2 = target.Body.EquipmentSlots.ToTempList();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is EquipmentSlot<BlueprintItemMechadendrite> equipmentSlot)
			{
				ItemEntity item = equipmentSlot.MaybeItem?.Blueprint.CreateEntity();
				if (item == null)
				{
					continue;
				}
				ItemSlot itemSlot = list2.FirstOrDefault((ItemSlot s) => s.CanInsertItem(item) && (!s.HasItem || s.CanRemoveItem()) && s.MaybeItem?.Blueprint == item.Blueprint);
				if (itemSlot != null)
				{
					if (itemSlot.HasItem)
					{
						itemSlot.RemoveItem();
					}
					itemSlot.InsertItem(item);
				}
				else
				{
					EquipmentSlot<BlueprintItemMechadendrite> equipmentSlot2 = new EquipmentSlot<BlueprintItemMechadendrite>(target);
					target?.Body.EquipmentSlots.Add(equipmentSlot2);
					target?.Body.AllSlots.Add(equipmentSlot2);
					target?.Body.Mechadendrites.Add(equipmentSlot2);
					equipmentSlot2.InsertItem(item);
				}
			}
			else
			{
				ItemSlot itemSlot2 = list[i];
				if (itemSlot2.MaybeItem != null)
				{
					list2[i].InsertItem(itemSlot2.MaybeItem.Blueprint.CreateEntity());
				}
			}
		}
	}

	public static BaseUnitEntity SummonCopy(BaseUnitEntity source, BlueprintUnit bp, SceneEntitiesState state, bool doNotCreateItems)
	{
		BaseUnitEntity baseUnitEntity;
		using (ContextData<DoNotCreateItems>.RequestIf(doNotCreateItems))
		{
			baseUnitEntity = bp.CreateEntity();
		}
		baseUnitEntity.ViewSettings.SetCustomPrefabGuid(source.ViewSettings.PrefabGuid);
		baseUnitEntity.Description.SetName(source.CharacterName);
		baseUnitEntity.Description.SetGender(source.Gender);
		baseUnitEntity.Asks.SetCustom(source.Asks.List);
		baseUnitEntity.Faction.Set(ConfigRoot.Instance.SystemMechanics.FactionCutsceneNeutral);
		baseUnitEntity.CombatGroup.Id = Uuid.Instance.CreateString();
		if (source.UISettings.PortraitBlueprint != null)
		{
			baseUnitEntity.UISettings.SetPortrait(source.UISettings.PortraitBlueprint);
		}
		else
		{
			baseUnitEntity.UISettings.SetPortrait(source.UISettings.Portrait);
		}
		baseUnitEntity.ViewSettings.SetDoll(source.ViewSettings.Doll);
		baseUnitEntity.GetOrCreate<UnitPartUnlootable>();
		baseUnitEntity.Progression.CopyFrom(source.Progression);
		CopyFacts(source, baseUnitEntity);
		CopyItems(source, baseUnitEntity);
		if (baseUnitEntity.Faction.IsPlayer)
		{
			baseUnitEntity.Inventory.EnsureOwn();
		}
		baseUnitEntity.AttachToViewOnLoad(null);
		Game.Instance.Controllers.EntitySpawner.SpawnEntityImmediately(baseUnitEntity, state, moveView: true);
		return baseUnitEntity;
	}

	public static bool IsCustomCompanion(this BaseUnitEntity _this)
	{
		return _this.OriginalBlueprint == ConfigRoot.Instance.SystemMechanics.CustomCompanion;
	}

	public static bool IsPregenCustomCompanion(this BaseUnitEntity unitEntity)
	{
		return ConfigRoot.Instance.CharGenRoot.IsBlueprintCompanionPregen(unitEntity.OriginalBlueprint);
	}

	public static bool IsInCompanionRoster(this MechanicEntity _this)
	{
		CompanionState companionState = _this.GetOptional<UnitPartCompanion>()?.State ?? CompanionState.None;
		if (!(_this is UnitEntity { IsMainCharacter: not false }) && companionState != CompanionState.InParty && companionState != CompanionState.InPartyDetached)
		{
			return companionState == CompanionState.Remote;
		}
		return true;
	}

	public static bool IsNavigatorCompanion(this BaseUnitEntity _this)
	{
		return _this.Progression.GetRank(ConfigRoot.Instance.SystemMechanics.NavigatorOccupation) > 0;
	}

	public static BaseUnitEntity CreatePreview(this BaseUnitEntity _this, bool createView)
	{
		return _this.Copy(createView, preview: true);
	}

	[CanBeNull]
	public static WeaponSlot GetThreatHandMelee(this MechanicEntity unit)
	{
		PartUnitBody bodyOptional = unit.GetBodyOptional();
		if (bodyOptional == null)
		{
			return null;
		}
		if (unit.IsThreatHandMelee(bodyOptional.PrimaryHand))
		{
			return bodyOptional.PrimaryHand;
		}
		if (0 == 0 && unit.IsThreatHandMelee(bodyOptional.SecondaryHand))
		{
			return bodyOptional.SecondaryHand;
		}
		foreach (WeaponSlot additionalLimb in bodyOptional.AdditionalLimbs)
		{
			if (unit.IsThreatHandMelee(additionalLimb))
			{
				return additionalLimb;
			}
		}
		return null;
	}

	[CanBeNull]
	public static WeaponSlot GetThreatHandRanged(this MechanicEntity unit)
	{
		PartUnitBody bodyOptional = unit.GetBodyOptional();
		if (bodyOptional != null && unit.IsThreatHandRanged(bodyOptional.PrimaryHand))
		{
			return bodyOptional.PrimaryHand;
		}
		return null;
	}

	[CanBeNull]
	public static WeaponSlot GetThreatHand(this MechanicEntity unit)
	{
		return unit.GetThreatHands().FirstOrDefault();
	}

	public static IEnumerable<WeaponSlot> GetThreatHands(this MechanicEntity unit)
	{
		using (ProfileScope.New("GetThreatHands"))
		{
			PartUnitBody body = unit.GetBodyOptional();
			if (body == null)
			{
				yield break;
			}
			if (unit.IsThreatHand(body.PrimaryHand))
			{
				yield return body.PrimaryHand;
			}
			if (unit.IsThreatHand(body.SecondaryHand))
			{
				yield return body.SecondaryHand;
			}
			foreach (WeaponSlot additionalLimb in body.AdditionalLimbs)
			{
				if (unit.IsThreatHand(additionalLimb))
				{
					yield return additionalLimb;
				}
			}
		}
	}

	private static bool IsThreatHandMelee(this MechanicEntity unit, WeaponSlot hand)
	{
		if (unit.IsThreatHand(hand))
		{
			return hand.Weapon.Blueprint.IsMelee;
		}
		return false;
	}

	private static bool IsThreatHandRanged(this MechanicEntity unit, WeaponSlot hand)
	{
		return hand.Weapon.Blueprint.IsRanged;
	}

	private static bool IsThreatHand(this MechanicEntity unit, WeaponSlot hand)
	{
		if (hand.HasWeapon)
		{
			if (!hand.Weapon.Blueprint.IsMelee)
			{
				return unit.Features.AllowAttackOfOpportunityWithRangedWeapon;
			}
			return true;
		}
		return false;
	}

	public static bool CanAttack(this MechanicEntity unit, ItemEntityWeapon weapon)
	{
		UnitPartAttackTypeRestriction optional = unit.GetOptional<UnitPartAttackTypeRestriction>();
		if (weapon != null && optional != null)
		{
			return optional.CanAttack(weapon.Blueprint.AttackType);
		}
		return true;
	}

	public static bool CanAttack(this BaseUnitEntity unit, Func<BaseUnitEntity, ItemEntityWeapon> weaponGetter)
	{
		UnitPartAttackTypeRestriction optional = unit.GetOptional<UnitPartAttackTypeRestriction>();
		if (optional == null)
		{
			return true;
		}
		ItemEntityWeapon itemEntityWeapon = weaponGetter?.Invoke(unit);
		if (itemEntityWeapon != null)
		{
			return optional.CanAttack(itemEntityWeapon.Blueprint.AttackType);
		}
		return true;
	}

	public static bool IsReach(this BaseUnitEntity unit, BaseUnitEntity enemy, WeaponSlot hand)
	{
		float num = hand.Weapon.ThreatRange;
		if ((float)unit.DistanceToInCells(enemy) <= unit.Corpulence + enemy.Corpulence + num)
		{
			return unit.Vision.HasLOS(enemy);
		}
		return false;
	}

	public static bool IsAttackOfOpportunityReach(this BaseUnitEntity attacker, BaseUnitEntity target, WeaponSlot hand)
	{
		using (ProfileScope.New("IsAttackOfOpportunityReach"))
		{
			float num = hand.Weapon.ThreatRange;
			return (float)attacker.DistanceToInCells(target) <= num && attacker.Vision.HasLOS(target) && attacker.HasMeleeLos(target);
		}
	}

	public static bool IsAttackOfOpportunityReach(this BaseUnitEntity attacker, GraphNode targetNode, Vector3 attackerPosition, IntRect targetSize, WeaponSlot hand)
	{
		using (ProfileScope.New("IsAttackOfOpportunityReach"))
		{
			int threatRange = hand.Weapon.ThreatRange;
			return WarhammerGeometryUtils.DistanceToInCells(attackerPosition, attacker.SizeRect, targetNode.Vector3Position(), targetSize) <= threatRange && attacker.Vision.HasLOS(targetNode, attackerPosition + LosCalculations.EyeShift) && LosCalculations.HasMeleeLos(attackerPosition, attacker.SizeRect, targetNode.Vector3Position(), targetSize);
		}
	}

	public static void SetUnitEquipmentColorRampIndex(this BaseUnitEntity unit, int rampIndex, bool secondary = false)
	{
		if (unit.ViewSettings.Doll == null)
		{
			unit.ViewSettings.SetDoll(new DollData());
		}
		if (secondary)
		{
			unit.ViewSettings.Doll.ClothesSecondaryIndex = rampIndex;
		}
		else
		{
			unit.ViewSettings.Doll.ClothesPrimaryIndex = rampIndex;
		}
		EventBus.RaiseEvent((IMechanicEntity)unit, (Action<IUnitVisualChangeHandler>)delegate(IUnitVisualChangeHandler h)
		{
			h.HandleUnitChangeEquipmentColor(rampIndex, secondary);
		}, isCheckRuntime: true);
	}

	public static void SetUnitEquipmentColorRampIndex(this BaseUnitEntity unit, RampColorPreset.IndexSet rampIndex)
	{
		if (unit.ViewSettings.Doll == null)
		{
			unit.ViewSettings.SetDoll(new DollData());
		}
		unit.ViewSettings.Doll.ClothesSecondaryIndex = rampIndex.SecondaryIndex;
		unit.ViewSettings.Doll.ClothesPrimaryIndex = rampIndex.PrimaryIndex;
		EventBus.RaiseEvent((IMechanicEntity)unit, (Action<IUnitVisualChangeHandler>)delegate(IUnitVisualChangeHandler h)
		{
			h.HandleUnitChangeEquipmentColor(rampIndex.PrimaryIndex, secondary: false);
		}, isCheckRuntime: true);
	}

	[CanBeNull]
	public static BaseUnitEntity GetSaddledUnit(this BaseUnitEntity unit)
	{
		return null;
	}

	[CanBeNull]
	public static BaseUnitEntity GetRider(this BaseUnitEntity unit)
	{
		return null;
	}

	public static int GetWeaponOptimalRange(this MechanicEntity unit, [CanBeNull] BlueprintItemWeapon weapon = null)
	{
		return SimpleBlueprintExtendAsObject.Or(weapon, null)?.AttackOptimalRange ?? 1;
	}

	public static DamageEstimate EstimateDamage(ItemEntityWeapon weapon, BaseUnitEntity target)
	{
		RuleCalculateStatsWeapon weaponStats = weapon.GetWeaponStats(weapon.Wielder ?? Game.Instance.DefaultUnit);
		IntermediateDamage intermediateDamage = weaponStats.ResultDamage.Copy();
		float num = weaponStats.ResultDamage.AverageValue;
		DamageEstimate result = default(DamageEstimate);
		result.Value = Math.Max(1, (int)num);
		result.BypassDR = true;
		result.Chunks = new IntermediateDamage[1] { intermediateDamage };
		return result;
	}

	public static Vector3 GetUnitSpawnerSize(BlueprintUnit unit)
	{
		Vector3 vector = new Vector3(1.35f, 1.35f, 1.35f);
		switch (unit?.Size ?? Size.Medium)
		{
		case Size.Fine:
			return vector * 0.1f;
		case Size.Diminutive:
			return vector * 0.3f;
		case Size.Tiny:
			return vector * 0.5f;
		case Size.Small:
			return vector * 0.7f;
		case Size.Large:
			return vector * 2f;
		case Size.Huge:
			return vector * 3f;
		case Size.Gargantuan:
			return vector * 4f;
		case Size.Colossal:
			return vector * 5f;
		case Size.Frigate_1x2:
			return vector * 1.2f;
		case Size.Cruiser_2x4:
			return vector * 2f;
		case Size.GrandCruiser_3x6:
			return vector * 3f;
		default:
			throw new ArgumentOutOfRangeException();
		case Size.Medium:
		case Size.Raider_1x1:
			return vector;
		}
	}

	public static bool IsStoryCompanion(this BaseUnitEntity unit)
	{
		return unit.Blueprint.GetComponent<UnitIsStoryCompanion>() != null;
	}

	public static void UpdateDropTransform(BaseUnitEntity ownerUnit, Transform loot, Vector3 rotation)
	{
		float scaleMultiplierBySize = GetScaleMultiplierBySize(ownerUnit);
		loot.localScale = new Vector3(scaleMultiplierBySize, scaleMultiplierBySize, scaleMultiplierBySize);
		loot.rotation = Quaternion.Euler(rotation);
	}

	public static float GetScaleMultiplierBySize(BaseUnitEntity ownerUnit)
	{
		BlueprintUnit blueprintUnit = ownerUnit?.Blueprint;
		if (blueprintUnit == null)
		{
			return 1f;
		}
		return blueprintUnit.Size switch
		{
			Size.Fine => 0.1f, 
			Size.Diminutive => 0.25f, 
			Size.Tiny => 0.5f, 
			Size.Small => 0.75f, 
			Size.Medium => 1f, 
			Size.Large => 1.5f, 
			Size.Huge => 2f, 
			Size.Gargantuan => 2.5f, 
			Size.Colossal => 3f, 
			_ => 1f, 
		};
	}

	public static void SnapToGrid(this IEnumerable<BaseUnitEntity> units)
	{
		foreach (BaseUnitEntity unit in units)
		{
			unit.MovementAgent.Blocker.Unblock();
		}
		foreach (BaseUnitEntity item in units.OrderByDescending((BaseUnitEntity v) => v.SizeRect.Height * v.SizeRect.Width))
		{
			try
			{
				GridNodeBase gridNodeBase = FindPositionForUnit(item);
				if (gridNodeBase != null)
				{
					item.Movable.ForceHasMotion = true;
					item.Position = gridNodeBase.Vector3Position();
				}
			}
			catch (Exception exception)
			{
				PFLog.Default.ExceptionWithReport(exception, null);
			}
			finally
			{
				item.MovementAgent.UpdateBlocker();
			}
		}
	}

	public static void SnapToGrid(this BaseUnitEntity unit)
	{
		UnitMovementAgentBase maybeMovementAgent = unit.MaybeMovementAgent;
		if (maybeMovementAgent == null)
		{
			return;
		}
		try
		{
			maybeMovementAgent.Blocker.Unblock();
			GridNodeBase gridNodeBase = FindPositionForUnit(unit);
			if (gridNodeBase != null)
			{
				unit.Movable.ForceHasMotion = true;
				unit.Position = gridNodeBase.Vector3Position();
			}
		}
		catch (Exception exception)
		{
			PFLog.Default.ExceptionWithReport(exception, null);
		}
		finally
		{
			maybeMovementAgent.UpdateBlocker();
		}
	}

	public static bool CanStandHere(this MechanicEntity unit, GraphNode node)
	{
		UnitMovementAgentBase maybeMovementAgent = unit.MaybeMovementAgent;
		if (maybeMovementAgent == null)
		{
			return true;
		}
		NodeList nodes = GridAreaHelper.GetNodes(node, unit.SizeRect);
		bool result = true;
		foreach (GridNodeBase item in nodes)
		{
			if (item == null || !item.Walkable || WarhammerBlockManager.Instance.NodeContainsAnyExcept(item, maybeMovementAgent.Blocker) || !IsNodeConnected(item, nodes))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private static bool IsNodeConnected(GridNodeBase node, IEnumerable<GridNodeBase> nodes)
	{
		if (!(node.Graph is GridGraph gridGraph))
		{
			return true;
		}
		for (int i = node.XCoordinateInGrid - 1; i < node.XCoordinateInGrid + 1; i++)
		{
			for (int j = node.ZCoordinateInGrid - 1; j < node.ZCoordinateInGrid + 1; j++)
			{
				if (i != node.XCoordinateInGrid || j != node.ZCoordinateInGrid)
				{
					GridNodeBase node2 = gridGraph.GetNode(i, j);
					if (nodes.Contains(node2) && !node.ContainsConnection(node2))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	[CanBeNull]
	private static GridNodeBase FindPositionForUnit(MechanicEntity unit)
	{
		GridNodeBase gridNodeBase = (GridNodeBase)(GraphNode)unit.CurrentNode;
		if (gridNodeBase == null)
		{
			PFLog.Default.ErrorWithReport($"UnitHelper.FindPositionForUnit: can't find origin node for {unit}");
			return null;
		}
		BlueprintAreaPart currentlyLoadedAreaPart = Game.Instance.CurrentlyLoadedAreaPart;
		Bounds? bounds = ((currentlyLoadedAreaPart == null) ? null : ObjectExtensions.Or(currentlyLoadedAreaPart.Bounds, null)?.MechanicBounds);
		if (gridNodeBase.ContainsPoint(unit.Position) && unit.CanStandHere(gridNodeBase) && (!bounds.HasValue || bounds.Value.ContainsXZ(gridNodeBase.Vector3Position())))
		{
			return gridNodeBase;
		}
		using (CloseToFarNodesEnumerator closeToFarNodesEnumerator = new CloseToFarNodesEnumerator(unit.Position))
		{
			while (closeToFarNodesEnumerator.MoveNext())
			{
				GridNodeBase current = closeToFarNodesEnumerator.Current;
				if (current != null && unit.CanStandHere(current) && (!bounds.HasValue || bounds.Value.ContainsXZ(current.Vector3Position())))
				{
					return current;
				}
			}
		}
		if (!unit.CanStandHere(gridNodeBase))
		{
			PFLog.Default.ErrorWithReport($"UnitHelper.FindPositionForUnit: can't find position for {unit}");
		}
		return gridNodeBase;
	}

	public static UnitMoveToProperParams TryCreateMoveCommandTB(this BaseUnitEntity unit, MoveCommandSettings settings, bool showMovePrediction)
	{
		MoveCommandStatus status;
		return unit.TryCreateMoveCommandTB(settings, showMovePrediction, out status);
	}

	public static UnitMoveToProperParams TryCreateMoveCommandTB(this BaseUnitEntity unit, MoveCommandSettings settings, bool showMovePrediction, out MoveCommandStatus status)
	{
		return TryCreateMoveCommandTBUnit(unit, settings, showMovePrediction, out status);
	}

	private static UnitMoveToProperParams TryCreateMoveCommandTBUnit(BaseUnitEntity unit, MoveCommandSettings settings, bool showMovePrediction, out MoveCommandStatus status)
	{
		if (!unit.CanMove)
		{
			status = MoveCommandStatus.CannotMove;
			return null;
		}
		WarhammerPathPlayer warhammerPathPlayer = PathfindingService.Instance.FindPathTB_Blocking(unit.View.MovementAgent, settings.Destination, limitRangeByActionPoints: false);
		using (PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, unit))
		{
			object obj;
			if (unit != null)
			{
				obj = Rulebook.Trigger(new RuleCalculateMovementCost(unit, warhammerPathPlayer));
			}
			else
			{
				obj = null;
			}
			int num = ((RuleCalculateMovementCost)obj)?.ResultPointCount ?? 0;
			float[] costPerEveryCell = ((RuleCalculateMovementCost)obj)?.ResultAPCostPerPoint ?? Array.Empty<float>();
			while (num > 0)
			{
				NodeList nodes = GridAreaHelper.GetNodes(warhammerPathPlayer.path[num - 1], unit.SizeRect);
				if (!WarhammerBlockManager.Instance.NodeContainsAnyExcept(nodes, unit.View.MovementAgent.Blocker))
				{
					break;
				}
				num--;
			}
			if (num < 2)
			{
				status = MoveCommandStatus.NotEnoughPoints;
				return null;
			}
			IEnumerable<GraphNode> nodes2 = warhammerPathPlayer.path.Take(num);
			ForcedPath forcedPath = ForcedPath.Construct(warhammerPathPlayer.vectorPath.Take(num), nodes2);
			Path path = UnitPathManager.Instance.GetPath(unit);
			if (path != null && path.vectorPath.SequenceEqual(forcedPath.vectorPath))
			{
				forcedPath.Claim(unit);
				forcedPath.Release(unit);
				status = MoveCommandStatus.SamePath;
				return null;
			}
			status = MoveCommandStatus.NewCommandCreated;
			UnitMoveToProperParams unitMoveToProperParams = CreateMoveCommandUnit(unit, settings, costPerEveryCell, forcedPath);
			if (showMovePrediction)
			{
				DrawMovePrediction(unit, forcedPath, costPerEveryCell, unitMoveToProperParams);
			}
			return unitMoveToProperParams;
		}
	}

	public static void DrawMovePrediction([NotNull] BaseUnitEntity unit, [NotNull] Path forcedPath, [CanBeNull] float[] costPerEveryCell, [CanBeNull] UnitCommandParams unitCommandParams = null)
	{
		if (forcedPath.vectorPath != null)
		{
			Game.Instance.GameCommandQueue.DrawMovePrediction(unit, forcedPath, costPerEveryCell, unitCommandParams);
		}
	}

	public static void DrawMovePredictionLocal(BaseUnitEntity unit, Path forcedPath, float[] costPerEveryCell)
	{
		UnitPathManager.Instance.RemovePath(unit);
		UnitPathManager.Instance.AddPath(unit, forcedPath, unit.Blueprint.WarhammerMovementApPerCell, unit.CombatState.MovementPoints, unit.CombatState.LastDiagonalCount % 2 == 1, costPerEveryCell);
		try
		{
			List<Vector3> vectorPath = forcedPath.vectorPath;
			Vector3 vector = vectorPath[vectorPath.Count - 1];
			Vector3 vector2;
			if (forcedPath.vectorPath.Count <= 1 || unit.SizeRect.Height == unit.SizeRect.Width)
			{
				vector2 = Vector3.zero;
			}
			else
			{
				List<Vector3> vectorPath2 = forcedPath.vectorPath;
				vector2 = (vector - vectorPath2[vectorPath2.Count - 2]).normalized;
			}
			Vector3 direction = vector2;
			UnitPredictionManager.Instance.SetHologramPosition(unit, vector, direction);
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
	}

	public static void ClearPrediction(BaseUnitEntity unit = null)
	{
		UnitPredictionManager.Instance.ClearAll();
		if (unit != null)
		{
			UnitPathManager.Instance.RemovePath(unit);
		}
		else
		{
			UnitPathManager.Instance.RemoveAllPaths();
		}
	}

	private static UnitMoveToProperParams CreateMoveCommandUnit(AbstractUnitEntity unit, MoveCommandSettings settings, float[] costPerEveryCell, ForcedPath forcedPath)
	{
		UnitMoveToProperParams unitMoveToProperParams = new UnitMoveToProperParams(forcedPath, unit.Blueprint.WarhammerMovementApPerCell, costPerEveryCell)
		{
			IsSynchronized = true
		};
		float num = forcedPath.Length();
		if (BuildModeUtility.IsDevelopment)
		{
			if (CheatsAnimation.SpeedForce > 0f)
			{
				unitMoveToProperParams.OverrideSpeed = CheatsAnimation.SpeedForce;
			}
			if (unit.IsInPlayerParty && unit.IsInCombat)
			{
				if (num >= (float)ConfigRoot.Instance.SystemMechanics.MinSprintDistanceInCombatCells * GraphParamsMechanicsCache.GridCellSize)
				{
					unitMoveToProperParams.MovementType = WalkSpeedType.Sprint;
				}
				else
				{
					unitMoveToProperParams.MovementType = WalkSpeedType.Run;
				}
			}
			else
			{
				unitMoveToProperParams.MovementType = (WalkSpeedType)CheatsAnimation.MoveType;
			}
		}
		else if (unit.IsInPlayerParty && unit.IsInCombat)
		{
			if (num >= (float)ConfigRoot.Instance.SystemMechanics.MinSprintDistanceInCombatCells * GraphParamsMechanicsCache.GridCellSize)
			{
				unitMoveToProperParams.MovementType = WalkSpeedType.Sprint;
			}
			else
			{
				unitMoveToProperParams.MovementType = WalkSpeedType.Run;
			}
		}
		return unitMoveToProperParams;
	}

	public static UnitMoveToParams CreateMoveCommandParamsRT(BaseUnitEntity unit, MoveCommandSettings settings, ForcedPath path)
	{
		UnitMoveToParams unitMoveToParams = new UnitMoveToParams(path, settings.Destination)
		{
			IsSynchronized = true
		};
		float num = path.Length();
		if (BuildModeUtility.IsDevelopment)
		{
			if (CheatsAnimation.SpeedForce > 0f)
			{
				unitMoveToParams.OverrideSpeed = CheatsAnimation.SpeedForce;
			}
			if (unit.IsInPlayerParty && !unit.IsInCombat)
			{
				if (num > (float)ConfigRoot.Instance.SystemMechanics.MinSprintDistance)
				{
					unitMoveToParams.MovementType = WalkSpeedType.Sprint;
				}
				else if (num < (float)ConfigRoot.Instance.SystemMechanics.MaxWalkDistance)
				{
					unitMoveToParams.MovementType = WalkSpeedType.Walk;
				}
				else
				{
					unitMoveToParams.MovementType = WalkSpeedType.Run;
				}
			}
			else
			{
				unitMoveToParams.MovementType = (WalkSpeedType)CheatsAnimation.MoveType;
			}
		}
		else if (unit.IsInPlayerParty && !unit.IsInCombat)
		{
			if (num > (float)ConfigRoot.Instance.SystemMechanics.MinSprintDistance)
			{
				unitMoveToParams.MovementType = WalkSpeedType.Sprint;
			}
			else if (num < (float)ConfigRoot.Instance.SystemMechanics.MaxWalkDistance)
			{
				unitMoveToParams.MovementType = WalkSpeedType.Walk;
			}
			else
			{
				unitMoveToParams.MovementType = WalkSpeedType.Run;
			}
		}
		return unitMoveToParams;
	}

	public static UnitFollowParams CreateUnitFollowCommandParamsRT(BaseUnitEntity unit, MoveCommandSettings settings)
	{
		bool isSynchronized = !ContextData<GameCommandContext>.Current && !ContextData<UnitCommandContext>.Current;
		return new UnitFollowParams(settings.FollowedUnit, settings.Destination)
		{
			IsSynchronized = isSynchronized
		};
	}

	[Cheat(Name = "respec", Description = "Respec selected unit", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	[UsedImplicitly]
	public static void CheatRespecUnit()
	{
		RespecCompanion respecCompanion = new RespecCompanion();
		respecCompanion.ForFree = true;
		respecCompanion.Run();
	}
}
