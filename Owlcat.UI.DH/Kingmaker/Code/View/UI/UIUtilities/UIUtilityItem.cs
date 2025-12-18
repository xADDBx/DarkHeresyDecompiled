using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Code.View.UI.MVVM.Tooltip;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Designers;
using Kingmaker.Designers.Mechanics.Facts.Damage;
using Kingmaker.DLC;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Enum;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.UI;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.Utility.UnitDescription;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using WebSocketSharp;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UIUtilityItem
{
	public struct ArmorData
	{
		public int DamageReduction;

		public int Durability;

		public int RangedHitChanceBonus;

		public int MovePointAdjustment;

		[Obsolete("Defence")]
		public string ArmorDodgePenalty;

		[Obsolete("Defence")]
		public string ArmourDodgeChanceDescription;

		public string ArmorDamageReduceDescription;
	}

	public class UIPatternData
	{
		public PatternGridData PatternCells;

		public Vector2Int[] MainCellsIndexes;

		public Vector2Int? OwnerCell;
	}

	public class UIScatterHitChanceData
	{
		public int MainLineClose;

		public int ScatterClose;

		public int MainLine;

		public int ScatterNear;

		public int ScatterFar;
	}

	public class UIAbilityData
	{
		public BlueprintAbility BlueprintAbility;

		public string AttackType;

		public string DamageText;

		public string BaseDamageText;

		public int MinDamage;

		public bool IsSpaceCombatAbility;

		public bool IsReload;

		public ItemEntityWeapon Weapon;

		public int MaxDamage;

		[Obsolete]
		public int Penetration;

		public bool IsRange;

		public bool IsScatter;

		public int BurstAttacksCount;

		public string CostAP;

		public UIPatternData PatternData;

		public int? HitChance;

		public UIScatterHitChanceData ScatterHitChanceData;

		public IEnumerable<UIProperty> UIProperties;

		public string Name => BlueprintAbility.Name;

		public Sprite Icon => BlueprintAbility.Icon;
	}

	private static readonly UIPatternData SingleShotPatternData = new UIPatternData
	{
		PatternCells = PatternGridData.Create(new HashSet<Vector2Int> { Vector2Int.zero }, disposable: false),
		MainCellsIndexes = new Vector2Int[1] { Vector2Int.zero }
	};

	private static readonly UIPatternData ScatterShotPatternData = new UIPatternData
	{
		PatternCells = PatternGridData.Create(new HashSet<Vector2Int>
		{
			new Vector2Int(1, 3),
			new Vector2Int(2, 3),
			new Vector2Int(3, 3),
			new Vector2Int(0, 2),
			new Vector2Int(1, 2),
			new Vector2Int(2, 2),
			new Vector2Int(3, 2),
			new Vector2Int(0, 1),
			new Vector2Int(1, 1),
			new Vector2Int(2, 1),
			new Vector2Int(3, 1),
			new Vector2Int(0, 0),
			new Vector2Int(1, 0),
			new Vector2Int(2, 0)
		}, disposable: false),
		MainCellsIndexes = new Vector2Int[4]
		{
			new Vector2Int(3, 3),
			new Vector2Int(2, 2),
			new Vector2Int(1, 1),
			new Vector2Int(0, 0)
		}
	};

	private static readonly UIPatternData StratagemPatternData = new UIPatternData
	{
		PatternCells = PatternGridData.Create(new HashSet<Vector2Int>
		{
			new Vector2Int(0, 3),
			new Vector2Int(1, 3),
			new Vector2Int(2, 3),
			new Vector2Int(3, 3),
			new Vector2Int(0, 2),
			new Vector2Int(1, 2),
			new Vector2Int(2, 2),
			new Vector2Int(3, 2),
			new Vector2Int(0, 1),
			new Vector2Int(1, 1),
			new Vector2Int(2, 1),
			new Vector2Int(3, 1),
			new Vector2Int(0, 0),
			new Vector2Int(1, 0),
			new Vector2Int(2, 0),
			new Vector2Int(3, 0)
		}, disposable: false),
		MainCellsIndexes = Array.Empty<Vector2Int>()
	};

	private static AbilityData _cachedAbilityData;

	public static bool CanReadItem(ItemEntity item)
	{
		if (Game.Instance.PartySharedInventory.Collection.Contains(item))
		{
			return true;
		}
		if (item.Owner == null || item.Owner.IsDead)
		{
			return true;
		}
		return false;
	}

	public static bool CanReadItem(BlueprintItem blueprintItem)
	{
		if (Game.Instance.PartySharedInventory.Collection.Contains(blueprintItem))
		{
			return true;
		}
		return false;
	}

	public static ArmorData GetArmorData(ItemEntityArmor armor)
	{
		using (GameLogContext.Scope)
		{
			float num = armor.Blueprint.Category switch
			{
				WarhammerArmorCategory.Power => 0.75f, 
				WarhammerArmorCategory.Heavy => 0.5f, 
				WarhammerArmorCategory.Medium => 0.75f, 
				_ => 0f, 
			};
			ArmorData armorData = default(ArmorData);
			armorData.DamageReduction = armor.Blueprint.ArmorDamageReduction;
			armorData.Durability = armor.Blueprint.ArmorDurability;
			armorData.MovePointAdjustment = 0;
			armorData.RangedHitChanceBonus = 0;
			armorData.ArmorDodgePenalty = ((num > 0f) ? $"{num}" : string.Empty);
			ArmorData result = armorData;
			if (((armor.Wielder as UnitEntity) ?? (Game.Instance.Controllers?.SelectionCharacter?.SelectedUnitInUI?.Value as UnitEntity)) == null)
			{
				return result;
			}
			GameLogContext.ResultDamage = 0;
			result.ArmorDamageReduceDescription = UIStrings.Instance.Tooltips.ArmourDamageReduceDescription;
			result.ArmourDodgeChanceDescription = UIStrings.Instance.Tooltips.ArmourDodgeChanceDescription;
			return result;
		}
	}

	public static UIAbilityData GetUIAbilityData(BlueprintAbility blueprintAbility, BlueprintItem blueprintItem, MechanicEntity caster = null)
	{
		ItemEntity itemEntity = null;
		if (blueprintItem != null)
		{
			using (ContextData<DisableStatefulRandomContext>.Request())
			{
				itemEntity = blueprintItem.CreateEntity();
			}
		}
		return GetUIAbilityData(blueprintAbility, itemEntity, caster);
	}

	public static UIAbilityData GetUIAbilityData(BlueprintAbility blueprintAbility, ItemEntity itemEntity = null, MechanicEntity caster = null)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			string damageText = string.Empty;
			string baseDamageText = string.Empty;
			int minDamage = 0;
			int maxDamage = 0;
			int? hitChance = 0;
			UIScatterHitChanceData scatterHitChanceData = null;
			if (caster == null)
			{
				caster = itemEntity?.Owner ?? UtilityParty.GetCurrentSelectedUnit() ?? Game.Instance.DefaultUnit;
			}
			if (caster == null || caster.IsDisposed)
			{
				return null;
			}
			AbilityData abilityData = CreateAbilityData(blueprintAbility, itemEntity, caster);
			RuleCalculateStatsWeapon weaponStats = abilityData.GetWeaponStats();
			BlueprintItemWeapon blueprintItemWeapon = abilityData.Weapon?.Blueprint;
			if (blueprintItemWeapon != null)
			{
				damageText = (abilityData.Blueprint.IsBurst ? $"({weaponStats.ResultDamage}-{blueprintItemWeapon.DamageMax})x{abilityData.RateOfFire}" : $"{weaponStats.ResultDamage}-{blueprintItemWeapon.DamageMax}");
				baseDamageText = $"{weaponStats.ResultDamage.MinValueBase}-{weaponStats.ResultDamage.MaxValueBase}";
				int minValue = weaponStats.ResultDamage.MinValue;
				int damageMax = blueprintItemWeapon.DamageMax;
				minDamage = minValue;
				maxDamage = damageMax;
			}
			if ((bool)(UnityEngine.Object)(object)AstarPath.active)
			{
				DamagePredictionData damagePrediction = abilityData.GetDamagePrediction(Game.Instance.DefaultUnit, Game.Instance.DefaultUnit.Position);
				if (damagePrediction != null)
				{
					damageText = ((damagePrediction.MinDamage != damagePrediction.MaxDamage) ? $"{damagePrediction.MinDamage}-{damagePrediction.MaxDamage}" : $"{damagePrediction.MinDamage}");
					int minValue = damagePrediction.MinDamage;
					int maxDamage2 = damagePrediction.MaxDamage;
					minDamage = minValue;
					maxDamage = maxDamage2;
				}
			}
			if (abilityData.Weapon != null)
			{
				if (abilityData.IsBurst)
				{
					scatterHitChanceData = new UIScatterHitChanceData
					{
						MainLine = 0,
						ScatterNear = 0,
						ScatterFar = 0,
						MainLineClose = 0,
						ScatterClose = 0
					};
				}
				else
				{
					hitChance = GetHitChanceRoll(caster, abilityData)?.ResultHitChance;
				}
			}
			return new UIAbilityData
			{
				BlueprintAbility = blueprintAbility,
				MinDamage = minDamage,
				MaxDamage = maxDamage,
				DamageText = damageText,
				BaseDamageText = baseDamageText,
				Penetration = 0,
				CostAP = $"{abilityData.CalculateActionPointCost()} {UIStrings.Instance.Tooltips.AP.Text}",
				PatternData = GetAbilityPatternData(abilityData, itemEntity),
				HitChance = hitChance,
				ScatterHitChanceData = scatterHitChanceData,
				IsRange = (blueprintItemWeapon?.IsRanged ?? false),
				IsScatter = abilityData.IsBurst,
				BurstAttacksCount = abilityData.BurstAttacksCount,
				UIProperties = abilityData.GetUIProperties(itemEntity),
				IsSpaceCombatAbility = false,
				IsReload = IsReload(abilityData),
				Weapon = abilityData.Weapon
			};
		}
	}

	public static AbilityData CreateAbilityData(BlueprintAbility blueprintAbility, ItemEntity itemEntity, MechanicEntity caster)
	{
		if (_cachedAbilityData != null && _cachedAbilityData.Blueprint.Equals(blueprintAbility) && _cachedAbilityData.SourceItem.Equals(itemEntity) && _cachedAbilityData.Caster.Equals(caster))
		{
			return _cachedAbilityData;
		}
		int indexInItemSettings = 0;
		ItemEntityWeapon itemEntityWeapon = itemEntity as ItemEntityWeapon;
		if (itemEntityWeapon != null)
		{
			int val = itemEntityWeapon.Blueprint.WeaponAbilities.Select((WeaponAbility i) => i.Ability).IndexOf(blueprintAbility);
			indexInItemSettings = Math.Max(0, val);
		}
		_cachedAbilityData = new AbilityData(blueprintAbility, caster, indexInItemSettings)
		{
			OverrideWeapon = itemEntityWeapon
		};
		return _cachedAbilityData;
	}

	public static RuleCalculateHitChances GetHitChanceRoll(MechanicEntity caster, AbilityData abilityData)
	{
		try
		{
			return Rulebook.Trigger(new RuleCalculateHitChances(caster, Game.Instance.DefaultUnit, abilityData, 0, Vector3.zero, Vector3.zero));
		}
		catch (Exception ex)
		{
			PFLog.UI.Error(ex);
			return null;
		}
	}

	private static UIPatternData GetAbilityPatternData(AbilityData ability, ItemEntity itemEntity)
	{
		AoEPattern aoEPattern = ability.Blueprint.PatternSettings?.Pattern;
		if (aoEPattern != null)
		{
			PatternGridData gridData = aoEPattern.GetGridData(Vector2.up);
			using PatternGridData.Enumerator enumerator = gridData.GetEnumerator();
			enumerator.MoveNext();
			Vector2Int[] mainCellsIndexes = ((itemEntity != null) ? Array.Empty<Vector2Int>() : new Vector2Int[1]
			{
				new Vector2Int(0, 0)
			});
			return new UIPatternData
			{
				PatternCells = gridData,
				MainCellsIndexes = mainCellsIndexes,
				OwnerCell = ((ability.TargetAnchor == AbilityTargetAnchor.Owner) ? new Vector2Int?(new Vector2Int(0, 0)) : ((ability.RangeCells == 1) ? new Vector2Int?(new Vector2Int(0, -1)) : null))
			};
		}
		if (ability.IsBurst)
		{
			return ScatterShotPatternData;
		}
		if (ability.Blueprint.IsStratagem)
		{
			return StratagemPatternData;
		}
		return SingleShotPatternData;
	}

	private static void FillItemRestrictions(ItemTooltipData data, BlueprintItem blueprintItem)
	{
		BaseUnitEntity unit = UtilityParty.GetCurrentSelectedUnit() ?? Game.Instance.Player.MainCharacterEntity;
		foreach (EquipmentRestriction component in blueprintItem.GetComponents<EquipmentRestriction>())
		{
			EquipmentRestrictionHasFacts equipmentRestrictionHasFacts = component as EquipmentRestrictionHasFacts;
			if (equipmentRestrictionHasFacts == null)
			{
				if (!(component is EquipmentRestrictionStat equipmentRestrictionStat))
				{
					if (component is EquipmentRestrictionMachineTrait equipmentRestrictionMachineTrait)
					{
						string text = LocalizedTexts.Instance.Stats.GetText(StatType.MachineTrait);
						RestrictionItem restrictionItem = new RestrictionItem
						{
							Key = text,
							Value = equipmentRestrictionMachineTrait.MinRank.ToString(),
							MeetPrerequisite = component.CanBeEquippedBy(unit)
						};
						data.Restrictions.Add(new RestrictionData(restrictionItem, inverted: false, all: false, equipmentRestrictionMachineTrait.CanBeEquippedBy(unit)));
					}
				}
				else
				{
					string text2 = LocalizedTexts.Instance.Stats.GetText(equipmentRestrictionStat.Stat);
					RestrictionItem restrictionItem2 = new RestrictionItem
					{
						Key = text2,
						Value = equipmentRestrictionStat.MinValue.ToString(),
						MeetPrerequisite = component.CanBeEquippedBy(unit)
					};
					data.Restrictions.Add(new RestrictionData(restrictionItem2, inverted: false, all: false, equipmentRestrictionStat.CanBeEquippedBy(unit)));
				}
				continue;
			}
			List<RestrictionItem> list = (from fact in equipmentRestrictionHasFacts.Facts
				where !(fact is BlueprintFeature { HideInUI: not false }) && !fact.IsDlcRestricted()
				select fact into unitFact
				select new RestrictionItem
				{
					UnitFact = unitFact,
					MeetPrerequisite = equipmentRestrictionHasFacts.CheckFactRestriction(unit, unitFact)
				}).ToList();
			if (equipmentRestrictionHasFacts.All)
			{
				IEnumerable<RestrictionData> collection = list.Select((RestrictionItem i) => new RestrictionData(i, equipmentRestrictionHasFacts.Inverted, equipmentRestrictionHasFacts.All, equipmentRestrictionHasFacts.CanBeEquippedBy(unit)));
				data.Restrictions.AddRange(collection);
			}
			else
			{
				data.Restrictions.Add(new RestrictionData(list, equipmentRestrictionHasFacts.Inverted, equipmentRestrictionHasFacts.All, equipmentRestrictionHasFacts.CanBeEquippedBy(unit)));
			}
		}
	}

	private static void FillWeaponDamage(ItemTooltipData data, RuleCalculateStatsWeapon defaultWeaponStats, RuleCalculateStatsWeapon equippedWeaponStats, ItemEntityWeapon weapon)
	{
		(int, int) damageMinMax = GetDamageMinMax(defaultWeaponStats.ResultDamage);
		data.Texts[TooltipElement.Damage] = $"{damageMinMax.Item1}-{damageMinMax.Item2}";
		data.CompareData[TooltipElement.Damage] = new CompareData
		{
			Value = damageMinMax.Item2 + damageMinMax.Item1
		};
		if (equippedWeaponStats != null)
		{
			(int, int) damageMinMax2 = GetDamageMinMax(equippedWeaponStats.ResultDamage);
			data.Texts[TooltipElement.EquipDamage] = $"{damageMinMax2.Item1}-{damageMinMax2.Item2}";
			(int, int) baseDamageMinMax = GetBaseDamageMinMax(equippedWeaponStats.ResultDamage);
			data.Texts[TooltipElement.BaseDamage] = $"{baseDamageMinMax.Item1}-{baseDamageMinMax.Item2}";
			data.CompareData[TooltipElement.EquipDamage] = new CompareData
			{
				Value = damageMinMax2.Item2 + damageMinMax2.Item1
			};
			equippedWeaponStats.ResultDamage.Modifiers.ValueModifiersList.Where((Modifier m) => m.Stat != StatType.Unknown).ForEach(delegate(Modifier mod)
			{
				data.BonusDamageFromStat[mod.Stat] = UIUtilityText.AddSign(mod.Value);
			});
		}
		else
		{
			(int, int) baseDamageMinMax2 = GetBaseDamageMinMax(defaultWeaponStats.ResultDamage);
			data.Texts[TooltipElement.BaseDamage] = $"{baseDamageMinMax2.Item1}-{baseDamageMinMax2.Item2}";
		}
	}

	private static void FillWeaponStats(ItemTooltipData data, ItemEntityWeapon weapon)
	{
		if (weapon.Blueprint.IsRanged)
		{
			data.Texts[TooltipElement.MaxDistance] = weapon.AttackRange.ToString();
		}
		int resultAdditionalSingleHitChance = weapon.GetWeaponStats().ResultAdditionalSingleHitChance;
		data.Texts[TooltipElement.SingleAdditionalHitChance] = ((resultAdditionalSingleHitChance > 0) ? UIUtilityText.AddPercentTo(resultAdditionalSingleHitChance) : string.Empty);
		int resultAdditionalBurstHitChance = weapon.GetWeaponStats().ResultAdditionalBurstHitChance;
		data.Texts[TooltipElement.BurstAdditionalHitChance] = ((resultAdditionalBurstHitChance > 0) ? UIUtilityText.AddPercentTo(resultAdditionalBurstHitChance) : string.Empty);
		int resultAdditionalAoeHitChance = weapon.GetWeaponStats().ResultAdditionalAoeHitChance;
		data.Texts[TooltipElement.AoeAdditionalHitChance] = ((resultAdditionalAoeHitChance > 0) ? UIUtilityText.AddPercentTo(resultAdditionalAoeHitChance) : string.Empty);
	}

	private static void FillWeaponAbilities(ItemTooltipData data, ItemEntityWeapon weapon)
	{
		foreach (WeaponAbility weaponAbility in weapon.Blueprint.WeaponAbilities)
		{
			try
			{
				data.Abilities.Add(GetUIAbilityData(weaponAbility.Ability, weapon));
			}
			catch (Exception ex)
			{
				LogChannel.Default.Error(ex);
			}
		}
	}

	private static void FillEquipmentDamage(ItemTooltipData data, BlueprintItemEquipment itemEquipment)
	{
		BlueprintAbility blueprintAbility = itemEquipment.Abilities.FirstOrDefault();
		if (blueprintAbility != null)
		{
			UIAbilityData uIAbilityData = GetUIAbilityData(blueprintAbility);
			data.Texts[TooltipElement.Damage] = uIAbilityData.DamageText;
			data.CompareData[TooltipElement.Damage] = new CompareData
			{
				Value = uIAbilityData.MaxDamage + uIAbilityData.MinDamage
			};
			data.Texts[TooltipElement.Penetration] = uIAbilityData.Penetration.ToString();
			data.CompareData[TooltipElement.Penetration] = new CompareData
			{
				Value = uIAbilityData.Penetration
			};
		}
	}

	private static void FillEquipmentAbilities(ItemTooltipData data, BlueprintItemEquipment itemEquipment)
	{
		foreach (BlueprintAbility ability in itemEquipment.Abilities)
		{
			data.Abilities.Add(GetUIAbilityData(ability));
		}
	}

	private static void FillEquipmentStatsBonuses(ItemTooltipData data, BlueprintItemEquipment itemEquipment)
	{
		foreach (AddFactToEquipmentWielder component in itemEquipment.GetComponents<AddFactToEquipmentWielder>())
		{
			foreach (AddStatBonus component2 in component.Fact.GetComponents<AddStatBonus>())
			{
				data.StatBonus.Add(component2.Stat, component2.Value);
			}
		}
	}

	private static string GetMechanicDescription(ItemEntity item)
	{
		if (item == null)
		{
			return string.Empty;
		}
		return item.Description;
	}

	private static string GetMechanicDescription(BlueprintItem blueprintItem)
	{
		if (blueprintItem == null)
		{
			return string.Empty;
		}
		return blueprintItem.Description;
	}

	private static string GetFlavorDescription(ItemEntity item)
	{
		if (item == null)
		{
			return string.Empty;
		}
		if (!CanReadItem(item))
		{
			return string.Empty;
		}
		return item.FlavorText;
	}

	private static string GetFlavorDescription(BlueprintItem blueprintItem)
	{
		if (blueprintItem == null)
		{
			return string.Empty;
		}
		if (!CanReadItem(blueprintItem))
		{
			return string.Empty;
		}
		return blueprintItem.FlavorText;
	}

	public static string GetHandUse(ItemEntity item)
	{
		if (item is ItemEntityWeapon)
		{
			return UIStrings.Instance.Tooltips.OneHanded;
		}
		return string.Empty;
	}

	public static string GetHandUse(BlueprintItem blueprintItem)
	{
		if (!(blueprintItem is BlueprintItemWeapon))
		{
			return string.Empty;
		}
		return UIStrings.Instance.Tooltips.OneHanded;
	}

	[Obsolete]
	private static void FillRange(ItemTooltipData data, ItemEntityWeapon weapon)
	{
		if (!weapon.Blueprint.IsMelee)
		{
			data.Texts[TooltipElement.Range] = weapon.AttackOptimalRange + "-" + weapon.AttackRange;
		}
	}

	private static void FillRateOfFire(ItemTooltipData data, ItemEntityWeapon weapon, RuleCalculateStatsWeapon weaponStats)
	{
		bool flag = !weapon.Blueprint.IsRanged;
		string value = Math.Max(1, weaponStats.ResultRateOfFire).ToString();
		data.Texts[flag ? TooltipElement.RateOfFireMelee : TooltipElement.RateOfFire] = value;
	}

	public static string GetItemOwnerName(ItemEntity item)
	{
		PartUnitDescription partUnitDescription = item.Owner?.GetDescriptionOptional();
		if (partUnitDescription != null)
		{
			return partUnitDescription.Name;
		}
		return string.Empty;
	}

	public static string GetWielderSlot(ItemEntity item)
	{
		return string.Empty ?? "";
	}

	public static string GetItemType(ItemEntity item)
	{
		if (!item.IsIdentified)
		{
			ItemsStrings items = LocalizedTexts.Instance.Items;
			if (!string.IsNullOrEmpty(item.Blueprint.SubtypeName))
			{
				return item.Blueprint.SubtypeName + " " + items.NotIdentifiedSuffix;
			}
			return items.NotIdentified;
		}
		return item.Blueprint.SubtypeName;
	}

	public static string GetItemType(BlueprintItem blueprintItem)
	{
		return blueprintItem.SubtypeName;
	}

	private static string GetEnhancementBonus(ItemEntity item)
	{
		if (!item.IsIdentified)
		{
			return string.Empty;
		}
		if (!(item is ItemEntityWeapon) && !(item is ItemEntityArmor) && !(item is ItemEntityShield))
		{
			return "none";
		}
		int itemEnhancementBonus = GameHelper.GetItemEnhancementBonus(item);
		if (itemEnhancementBonus == 0)
		{
			return string.Empty;
		}
		return UIUtilityText.AddSign(itemEnhancementBonus);
	}

	private static string GetQualities(ItemEntity item)
	{
		if (!item.IsIdentified)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (!(item is ItemEntityWeapon itemEntityWeapon))
		{
			return stringBuilder.ToString();
		}
		WeaponCategory category = itemEntityWeapon.Blueprint.Category;
		if (category.HasSubCategory(WeaponSubCategory.Finessable))
		{
			UIUtilityText.TryAddWordSeparator(stringBuilder, ", ");
			stringBuilder.Append(LocalizedTexts.Instance.WeaponSubCategories.GetText(WeaponSubCategory.Finessable));
		}
		if (category.HasSubCategory(WeaponSubCategory.Monk))
		{
			UIUtilityText.TryAddWordSeparator(stringBuilder, ", ");
			stringBuilder.Append(LocalizedTexts.Instance.WeaponSubCategories.GetText(WeaponSubCategory.Monk));
		}
		stringBuilder.Replace(" ,", ",");
		return stringBuilder.ToString();
	}

	public static string GetDamageDiceText(DiceFormula dice, int damageBonus)
	{
		if (damageBonus <= 0)
		{
			if (damageBonus != 0)
			{
				return $"{dice}{damageBonus}";
			}
			return $"{dice}";
		}
		return $"{dice}+{damageBonus}";
	}

	public static int MinDiceValue(UnitDescription.UIDamageInfo damageEntry)
	{
		return damageEntry.MinValue();
	}

	public static int MaxDiceValue(UnitDescription.UIDamageInfo damageEntry)
	{
		return damageEntry.MaxValue();
	}

	private static (int Min, int Max) GetDamageMinMax(IntermediateDamage damageInfo)
	{
		int val = 0;
		int item = Math.Max(0, damageInfo.MinValue);
		val = Math.Max(val, damageInfo.MaxValue);
		return (Min: item, Max: val);
	}

	private static (int Min, int Max) GetBaseDamageMinMax(IntermediateDamage damageInfo)
	{
		int val = 0;
		int item = Math.Max(0, damageInfo.MinValueBase);
		val = Math.Max(val, damageInfo.MaxValueBase);
		return (Min: item, Max: val);
	}

	private static void FillPenetration(ItemTooltipData data, ItemEntityWeapon weapon)
	{
		int warhammerPenetration = weapon.Blueprint.WarhammerPenetration;
		data.Texts[TooltipElement.Penetration] = warhammerPenetration.ToString();
		data.CompareData[TooltipElement.Penetration] = new CompareData
		{
			Value = warhammerPenetration
		};
	}

	private static string GetRangeType(ItemEntity item)
	{
		if (item is ItemEntityWeapon itemEntityWeapon)
		{
			return GetRangeType(itemEntityWeapon.Blueprint);
		}
		return string.Empty;
	}

	private static string GetRangeType(BlueprintItem blueprintItem)
	{
		if (!(blueprintItem is BlueprintItemWeapon blueprintItemWeapon))
		{
			return string.Empty;
		}
		AttackType val = ((!blueprintItemWeapon.IsMelee) ? AttackType.Ranged : AttackType.Melee);
		return LocalizedTexts.Instance.AttackTypes.GetText(val);
	}

	public static string GetProficiencyGroup(ItemEntity item)
	{
		if (!(item is ItemEntityWeapon itemEntityWeapon))
		{
			return string.Empty;
		}
		WeaponCategory category = itemEntityWeapon.Blueprint.Category;
		if (category.HasSubCategory(WeaponSubCategory.Exotic))
		{
			return LocalizedTexts.Instance.WeaponSubCategories.GetText(WeaponSubCategory.Exotic);
		}
		if (category.HasSubCategory(WeaponSubCategory.Martial))
		{
			return LocalizedTexts.Instance.WeaponSubCategories.GetText(WeaponSubCategory.Martial);
		}
		if (category.HasSubCategory(WeaponSubCategory.Simple))
		{
			return LocalizedTexts.Instance.WeaponSubCategories.GetText(WeaponSubCategory.Simple);
		}
		return string.Empty;
	}

	public static string GetItemGroup(ItemEntity item)
	{
		return ItemsFilter.GetItemType(item).ToString();
	}

	public static string GetItemGroup(BlueprintItem blueprintItem)
	{
		return ItemsFilter.GetItemType(blueprintItem).ToString();
	}

	public static bool CanEquipItem(ItemEntity item)
	{
		BaseUnitEntity unit = UtilityParty.GetCurrentSelectedUnit() ?? Game.Instance.Player.MainCharacterEntity;
		return UIInventoryHelper.CanEquipItem(item, unit);
	}

	public static bool CanInsertItem(ItemEntity item)
	{
		return (UtilityParty.GetCurrentSelectedUnit() ?? Game.Instance.Player.MainCharacterEntity).Body.AllSlots.Any((ItemSlot sl) => sl.IsItemSupported(item));
	}

	public static bool IsEquipPossible(ItemEntity item)
	{
		BaseUnitEntity owner = UtilityParty.GetCurrentSelectedUnit() ?? Game.Instance.Player.MainCharacterEntity;
		if (item.Blueprint is BlueprintItemEquipment)
		{
			return item.CanBeEquippedBy(owner);
		}
		return false;
	}

	public static bool[] GetEquipPosibility(ItemEntity item)
	{
		BaseUnitEntity baseUnitEntity = UtilityParty.GetCurrentSelectedUnit() ?? Game.Instance?.Player?.MainCharacterEntity;
		bool flag = true;
		bool flag2 = false;
		if (item != null && baseUnitEntity != null)
		{
			if (item.Blueprint is BlueprintItemEquipment)
			{
				flag = item.CanBeEquippedBy(baseUnitEntity);
			}
			return new bool[2] { flag, flag2 };
		}
		return new bool[2] { flag, flag2 };
	}

	public static ItemTooltipData GetItemTooltipData(ItemEntity item)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			ItemTooltipData itemTooltipData = new ItemTooltipData(item);
			ItemEntityUsable usable = itemTooltipData.Usable;
			itemTooltipData.Texts[TooltipElement.Name] = item.Name;
			itemTooltipData.Texts[TooltipElement.Count] = (item.IsStackable ? item.Count.ToString() : "");
			itemTooltipData.Texts[TooltipElement.ItemType] = GetItemGroup(item);
			itemTooltipData.Texts[TooltipElement.ItemCost] = UIUtilityText.GetCostFormatted(item.Blueprint.Cost);
			itemTooltipData.Texts[TooltipElement.Subname] = GetItemType(item);
			try
			{
				itemTooltipData.Texts[TooltipElement.Price] = Game.Instance.TradeLogic.GetItemCost(item).ToString();
			}
			catch (Exception arg)
			{
				PFLog.UI.Error($"Cannot calculate item cost (probably item is deleted).\n{arg}");
				itemTooltipData.Texts[TooltipElement.Price] = "0";
			}
			itemTooltipData.Texts[TooltipElement.Wielder] = GetItemOwnerName(item);
			itemTooltipData.Texts[TooltipElement.WielderSlot] = GetWielderSlot(item);
			itemTooltipData.Texts[TooltipElement.Twohanded] = GetHandUse(item);
			FillItemRestrictions(itemTooltipData, item.Blueprint);
			if (item.Blueprint is BlueprintItemWeapon blueprintItemWeapon)
			{
				StatsStrings stats = LocalizedTexts.Instance.Stats;
				itemTooltipData.Texts[TooltipElement.WeaponCategory] = stats.GetText(blueprintItemWeapon.Category);
				itemTooltipData.Texts[TooltipElement.WeaponFamily] = stats.GetText(blueprintItemWeapon.Family);
			}
			if (item.Blueprint is BlueprintItemNote || item.Blueprint is BlueprintItemKey)
			{
				if (CanReadItem(item))
				{
					if (string.IsNullOrEmpty(GetFlavorDescription(item)))
					{
						itemTooltipData.Texts[TooltipElement.ArtisticDescription] = GetMechanicDescription(item);
						itemTooltipData.Texts[TooltipElement.ShortDescription] = GetMechanicDescription(item);
					}
					else
					{
						itemTooltipData.Texts[TooltipElement.ArtisticDescription] = GetFlavorDescription(item);
						itemTooltipData.Texts[TooltipElement.ShortDescription] = GetFlavorDescription(item);
					}
				}
			}
			else
			{
				itemTooltipData.Texts[TooltipElement.ArtisticDescription] = GetFlavorDescription(item);
				itemTooltipData.Texts[TooltipElement.ShortDescription] = GetMechanicDescription(item);
			}
			string value = FillEnchantmentDescription(item, itemTooltipData);
			if (item.IsIdentified && usable != null)
			{
				itemTooltipData.Texts[TooltipElement.Charges] = usable.Charges.ToString();
				itemTooltipData.Texts[TooltipElement.CasterLevel] = string.Empty;
				BlueprintAbility blueprintAbility = usable.Blueprint.Abilities.FirstOrDefault();
				if (blueprintAbility != null)
				{
					itemTooltipData.Texts[TooltipElement.Cooldown] = blueprintAbility.CooldownRounds.ToString();
					itemTooltipData.Texts[TooltipElement.Target] = blueprintAbility.GetTarget(-1, item.Owner);
					itemTooltipData.BlueprintAbility = blueprintAbility;
					itemTooltipData.Texts[TooltipElement.ShortDescription] = usable.Abilities.FirstOrDefault()?.Data.ShortenedDescription ?? blueprintAbility?.GetShortenedDescription();
					itemTooltipData.Texts[TooltipElement.LongDescription] = usable.Abilities.FirstOrDefault()?.Data.Description ?? blueprintAbility?.GetShortenedDescription();
					itemTooltipData.Texts[TooltipElement.SpellDescriptor] = string.Empty;
					itemTooltipData.Texts[TooltipElement.CastingTime] = string.Empty;
					FillEquipmentAbilities(itemTooltipData, usable.Blueprint);
					FillEquipmentDamage(itemTooltipData, usable.Blueprint);
					FillEquipmentStatsBonuses(itemTooltipData, usable.Blueprint);
				}
			}
			if (!string.IsNullOrEmpty(value))
			{
				itemTooltipData.Texts[TooltipElement.EnchantmentsDescription] = value;
			}
			return itemTooltipData;
		}
	}

	public static ItemTooltipData GetItemTooltipData(ItemEntity item, bool replenishing)
	{
		ItemTooltipData itemTooltipData = GetItemTooltipData(item);
		if (replenishing)
		{
			itemTooltipData.Texts[TooltipElement.Replenishing] = UIStrings.Instance.Tooltips.ReplenishingItem;
		}
		return itemTooltipData;
	}

	public static ItemTooltipData GetItemTooltipData(BlueprintItem blueprintItem)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			ItemTooltipData itemTooltipData = new ItemTooltipData(blueprintItem);
			BlueprintItemEquipmentUsable blueprintUsable = itemTooltipData.BlueprintUsable;
			itemTooltipData.Texts[TooltipElement.Name] = blueprintItem.Name;
			itemTooltipData.Texts[TooltipElement.Count] = string.Empty;
			itemTooltipData.Texts[TooltipElement.ItemType] = GetItemGroup(blueprintItem);
			itemTooltipData.Texts[TooltipElement.ItemCost] = UIUtilityText.GetCostFormatted(blueprintItem.Cost);
			itemTooltipData.Texts[TooltipElement.Subname] = GetItemType(blueprintItem);
			itemTooltipData.Texts[TooltipElement.Wielder] = string.Empty;
			itemTooltipData.Texts[TooltipElement.WielderSlot] = string.Empty;
			itemTooltipData.Texts[TooltipElement.Twohanded] = GetHandUse(blueprintItem);
			FillItemRestrictions(itemTooltipData, blueprintItem);
			if (blueprintItem is BlueprintItemWeapon blueprintItemWeapon)
			{
				StatsStrings stats = LocalizedTexts.Instance.Stats;
				itemTooltipData.Texts[TooltipElement.WeaponCategory] = stats.GetText(blueprintItemWeapon.Category);
				itemTooltipData.Texts[TooltipElement.WeaponFamily] = stats.GetText(blueprintItemWeapon.Family);
			}
			if (blueprintItem is BlueprintItemNote || blueprintItem is BlueprintItemKey)
			{
				if (CanReadItem(blueprintItem))
				{
					if (string.IsNullOrEmpty(GetFlavorDescription(blueprintItem)))
					{
						itemTooltipData.Texts[TooltipElement.ArtisticDescription] = GetMechanicDescription(blueprintItem);
						itemTooltipData.Texts[TooltipElement.ShortDescription] = GetMechanicDescription(blueprintItem);
					}
					else
					{
						itemTooltipData.Texts[TooltipElement.ArtisticDescription] = GetFlavorDescription(blueprintItem);
						itemTooltipData.Texts[TooltipElement.ShortDescription] = GetFlavorDescription(blueprintItem);
					}
				}
			}
			else
			{
				itemTooltipData.Texts[TooltipElement.ArtisticDescription] = GetFlavorDescription(blueprintItem);
				itemTooltipData.Texts[TooltipElement.ShortDescription] = GetMechanicDescription(blueprintItem);
			}
			if (blueprintUsable != null)
			{
				itemTooltipData.Texts[TooltipElement.Charges] = blueprintUsable.Charges.ToString();
				itemTooltipData.Texts[TooltipElement.CasterLevel] = string.Empty;
				BlueprintAbility blueprintAbility = blueprintUsable.Abilities.FirstOrDefault();
				if (blueprintAbility != null)
				{
					_ = (string)ConfigRoot.Instance.LocalizedTexts.AbilityTargets.Personal;
					itemTooltipData.Texts[TooltipElement.Cooldown] = blueprintAbility.CooldownRounds.ToString();
					itemTooltipData.Texts[TooltipElement.Target] = blueprintAbility.GetTarget();
					itemTooltipData.BlueprintAbility = blueprintAbility;
					itemTooltipData.Texts[TooltipElement.ShortDescription] = SimpleBlueprintExtendAsObject.Or(blueprintAbility, null)?.GetShortenedDescription();
					itemTooltipData.Texts[TooltipElement.LongDescription] = SimpleBlueprintExtendAsObject.Or(blueprintAbility, null)?.Description;
					itemTooltipData.Texts[TooltipElement.SpellDescriptor] = string.Empty;
					itemTooltipData.Texts[TooltipElement.CastingTime] = string.Empty;
					FillEquipmentAbilities(itemTooltipData, blueprintUsable);
					FillEquipmentDamage(itemTooltipData, blueprintUsable);
					FillEquipmentStatsBonuses(itemTooltipData, blueprintUsable);
				}
			}
			return itemTooltipData;
		}
	}

	[Obsolete("Removed Enchantments")]
	private static string FillEnchantmentDescription(ItemEntity item, ItemTooltipData itemTooltipData)
	{
		string text = string.Empty;
		if (item is ItemEntityWeapon itemEntityWeapon)
		{
			RuleCalculateStatsWeapon weaponStats = itemEntityWeapon.GetWeaponStats();
			MechanicEntity mechanicEntity = item.Owner ?? UtilityParty.GetCurrentSelectedUnit();
			RuleCalculateStatsWeapon ruleCalculateStatsWeapon = ((mechanicEntity != null) ? itemEntityWeapon.GetWeaponStats(mechanicEntity) : null);
			RuleCalculateStatsWeapon weaponStats2 = ruleCalculateStatsWeapon ?? weaponStats;
			itemTooltipData.Texts[TooltipElement.AttackType] = GetRangeType(itemEntityWeapon);
			itemTooltipData.Texts[TooltipElement.ProficiencyGroup] = GetProficiencyGroup(itemEntityWeapon);
			text = FillWeaponQualities(itemTooltipData, itemEntityWeapon, text);
			FillPenetration(itemTooltipData, itemEntityWeapon);
			FillRateOfFire(itemTooltipData, itemEntityWeapon, weaponStats2);
			FillWeaponDamage(itemTooltipData, weaponStats, ruleCalculateStatsWeapon, itemEntityWeapon);
			FillWeaponStats(itemTooltipData, itemEntityWeapon);
			FillWeaponAbilities(itemTooltipData, itemEntityWeapon);
		}
		else if (item is ItemEntityArmor itemEntityArmor)
		{
			ArmorData armorData = GetArmorData(itemEntityArmor);
			itemTooltipData.Texts[TooltipElement.DamageReduction] = UIUtilityText.AddPercentTo(armorData.DamageReduction);
			itemTooltipData.CompareData[TooltipElement.DamageReduction] = new CompareData
			{
				Value = armorData.DamageReduction
			};
			itemTooltipData.HasValues[TooltipElement.DamageReduction] = armorData.DamageReduction > 0;
			itemTooltipData.Texts[TooltipElement.Durability] = armorData.Durability.ToString();
			itemTooltipData.CompareData[TooltipElement.Durability] = new CompareData
			{
				Value = armorData.Durability
			};
			itemTooltipData.HasValues[TooltipElement.Durability] = armorData.Durability > 0;
			itemTooltipData.Texts[TooltipElement.ArmorDodgePenalty] = armorData.ArmorDodgePenalty;
			itemTooltipData.Texts[TooltipElement.FullArmorClass] = LocalizedTexts.Instance.Stats.GetText(itemEntityArmor.Blueprint.Category);
			text = FillArmorEnchantments(itemTooltipData, itemEntityArmor, text);
			itemTooltipData.Texts[TooltipElement.ArmorCheckPenalty] = armorData.RangedHitChanceBonus.ToString();
			itemTooltipData.Texts[TooltipElement.ArcaneSpellFailure] = armorData.MovePointAdjustment.ToString();
			if (!armorData.ArmorDamageReduceDescription.IsNullOrEmpty())
			{
				itemTooltipData.Texts[TooltipElement.ArmorDamageReduceDescription] = armorData.ArmorDamageReduceDescription;
			}
			if (!armorData.ArmourDodgeChanceDescription.IsNullOrEmpty())
			{
				itemTooltipData.Texts[TooltipElement.ArmourDodgeChanceDescription] = armorData.ArmourDodgeChanceDescription;
			}
		}
		else if (item is ItemEntityShield itemEntityShield)
		{
			ArmorData armorData2 = GetArmorData(itemEntityShield.ArmorComponent);
			itemTooltipData.Texts[TooltipElement.ArmorDodgePenalty] = armorData2.ArmorDodgePenalty;
			itemTooltipData.Texts[TooltipElement.ArmorCheckPenalty] = armorData2.RangedHitChanceBonus.ToString();
			itemTooltipData.Texts[TooltipElement.ArcaneSpellFailure] = armorData2.MovePointAdjustment.ToString();
			text = FillShieldEnchantments(itemTooltipData, itemEntityShield, text);
		}
		return text;
	}

	[Obsolete("Removed Enchantments")]
	private static string FillShieldEnchantments(ItemTooltipData itemTooltipData, ItemEntityShield shield, string enchantmentDescription)
	{
		return enchantmentDescription;
	}

	[Obsolete("Removed Enchantments")]
	private static string FillArmorEnchantments(ItemTooltipData itemTooltipData, ItemEntityArmor armor, string enchantmentDescription)
	{
		if (armor.IsIdentified && GameHelper.GetItemEnhancementBonus(armor) > 0)
		{
			itemTooltipData.Texts[TooltipElement.Enhancement] = GetEnhancementBonus(armor);
		}
		return enchantmentDescription;
	}

	private static string FillWeaponQualities(ItemTooltipData itemTooltipData, ItemEntityWeapon weapon, string enchantmentDescription)
	{
		if (weapon.IsIdentified)
		{
			itemTooltipData.Texts[TooltipElement.Qualities] = GetQualities(weapon);
			if (GameHelper.GetItemEnhancementBonus(weapon) > 0)
			{
				itemTooltipData.Texts[TooltipElement.Enhancement] = GetEnhancementBonus(weapon);
			}
		}
		return enchantmentDescription;
	}

	[Obsolete("WH2-7361")]
	public static bool IsReload(BlueprintAbilityWrapper blueprintAbility)
	{
		return false;
	}

	[Obsolete("WH2-7361")]
	public static bool IsReload(BlueprintAbility blueprintAbility)
	{
		return false;
	}

	[Obsolete("WH2-7361")]
	public static bool IsReload(AbilityData abilityData)
	{
		return false;
	}

	[Obsolete("WH2-7361")]
	public static int GetMaxAbilityAmmo(ItemEntityWeapon weapon)
	{
		return 0;
	}

	public static (string, ItemHeaderType) GetItemHeaderText(ItemEntity item)
	{
		BaseUnitEntity currentSelectedUnit = UtilityParty.GetCurrentSelectedUnit();
		if (item == null || currentSelectedUnit == null)
		{
			return (string.Empty, ItemHeaderType.Header);
		}
		string itemOwnerName = GetItemOwnerName(item);
		if (item is ItemEntityWeapon || item is ItemEntityArmor || item is ItemEntityShield || item.Blueprint is BlueprintItemEquipment)
		{
			if (item.Owner == null)
			{
				if (item.CanBeEquippedBy(currentSelectedUnit))
				{
					return (UIUtilityText.UITooltips.CanBeEquip, ItemHeaderType.Header);
				}
				return (UIUtilityText.UITooltips.CannotbeEquip, ItemHeaderType.CanNotEquip);
			}
			if (UtilityParty.GetGroup().Contains(item.Owner))
			{
				return (itemOwnerName, ItemHeaderType.Equipped);
			}
		}
		else if (item is ItemEntityUsable itemEntityUsable)
		{
			if (itemEntityUsable.Blueprint.Abilities.FirstOrDefault() != null)
			{
				return (UIUtilityText.UITooltips.CannotbeUsed, ItemHeaderType.CanNotEquip);
			}
			return (UIUtilityText.UITooltips.CanBeUsed, ItemHeaderType.Header);
		}
		return (string.Empty, ItemHeaderType.Header);
	}

	public static Sprite GetItemIcon(ItemEntity item)
	{
		return item.Icon ?? ConfigRoot.Instance.UIConfig.UIIcons.DefaultItemIcon;
	}

	public static string GetArmorDamageBonus(ItemEntity item)
	{
		return GetModifierText(item?.Blueprint?.GetComponent<DamageModifier>()?.ArmorDamage);
	}

	public static string GetWoundDamageBonus(ItemEntity item)
	{
		return GetModifierText(item?.Blueprint?.GetComponent<DamageModifier>()?.Damage);
	}

	public static string GetVitalDamageBonus(ItemEntity item)
	{
		return GetModifierText(item?.Blueprint?.GetComponent<DamageModifier>()?.VitalDamage);
	}

	public static string GetModifierText(ContextValueModifierWithType modifier)
	{
		if (modifier == null)
		{
			return string.Empty;
		}
		if (modifier.Value == 0)
		{
			return string.Empty;
		}
		return AddSignToModifierText(modifier.Value, modifier.ModifierType);
	}

	public static string AddSignToModifierText(int value, ModifierType modifierType)
	{
		char prefix = modifierType.GetPrefix();
		return $"{prefix}{value}";
	}
}
