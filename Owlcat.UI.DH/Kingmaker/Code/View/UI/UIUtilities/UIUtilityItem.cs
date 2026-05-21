using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Designers;
using Kingmaker.Designers.Mechanics.Facts.Damage;
using Kingmaker.DLC;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
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
using R3;
using UnityEngine;

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

		public string DamageText;

		public string BaseDamageText;

		public int MinDamage;

		public int MaxDamage;

		public ItemEntityWeapon Weapon;

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

	private static CalculatorUnitPair s_CalculatorUnitPair;

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
			StatFactionModifierConfig[] fractionModifiers = armor.GetFractionModifiers();
			ArmorData armorData = default(ArmorData);
			armorData.DamageReduction = ApplyFractionModifier(armor.GetStatBaseValue(StatType.ItemArmorDamageReduction).Value, StatType.ItemArmorDamageReduction, fractionModifiers);
			armorData.Durability = ApplyFractionModifier(armor.GetStatBaseValue(StatType.ItemArmorAmount).Value, StatType.ItemArmorAmount, fractionModifiers);
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
			using (EvalContext.Build().Ability(abilityData).Push())
			{
				RuleCalculateStatsWeapon weaponStats = abilityData.GetWeaponStats();
				ItemEntityWeapon weapon = abilityData.Weapon;
				if (weapon != null)
				{
					int damageMax = weapon.DamageMax;
					damageText = (abilityData.Blueprint.IsBurst ? $"({weaponStats.ResultDamage}-{damageMax})x{abilityData.RateOfFire}" : $"{weaponStats.ResultDamage}-{damageMax}");
					baseDamageText = $"{weaponStats.ResultDamage.MinValueBase}-{weaponStats.ResultDamage.MaxValueBase}";
					int minValue = weaponStats.ResultDamage.MinValue;
					int num = damageMax;
					minDamage = minValue;
					maxDamage = num;
				}
				if ((bool)(UnityEngine.Object)(object)AstarPath.active)
				{
					DamagePredictionData damagePrediction = abilityData.GetDamagePrediction(Game.Instance.DefaultUnit, Game.Instance.DefaultUnit.Position);
					if (damagePrediction != null)
					{
						damageText = ((damagePrediction.MinDamage != damagePrediction.MaxDamage) ? $"{damagePrediction.MinDamage}-{damagePrediction.MaxDamage}" : $"{damagePrediction.MinDamage}");
						int num = damagePrediction.MinDamage;
						int maxDamage2 = damagePrediction.MaxDamage;
						minDamage = num;
						maxDamage = maxDamage2;
					}
				}
				if (abilityData.Weapon != null)
				{
					if (abilityData.IsBurst)
					{
						scatterHitChanceData = new UIScatterHitChanceData();
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
					CostAP = $"{abilityData.GetBaseActionPointCost()} {UIStrings.Instance.Tooltips.AP.Text}",
					PatternData = GetAbilityPatternData(abilityData, itemEntity),
					HitChance = hitChance,
					ScatterHitChanceData = scatterHitChanceData,
					IsRange = (weapon?.Blueprint.IsRanged ?? false),
					IsScatter = abilityData.IsBurst,
					BurstAttacksCount = abilityData.BurstAttacksCount,
					UIProperties = abilityData.GetUIProperties(itemEntity),
					Weapon = abilityData.Weapon
				};
			}
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
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			_cachedAbilityData = new AbilityData(blueprintAbility, caster, indexInItemSettings)
			{
				OverrideWeapon = itemEntityWeapon
			};
		}
		return _cachedAbilityData;
	}

	private static AbilityData GetWeaponPreviewAbility(ItemEntityWeapon weapon, MechanicEntity caster)
	{
		if (weapon == null || caster == null)
		{
			return null;
		}
		BlueprintAbility blueprintAbility = weapon.Blueprint.WeaponAbilities.FirstOrDefault((WeaponAbility wa) => wa?.Ability != null)?.Ability;
		if (blueprintAbility == null)
		{
			return null;
		}
		return CreateAbilityData(blueprintAbility, weapon, caster);
	}

	public static RuleCalculateHitChances GetHitChanceRoll(MechanicEntity caster, AbilityData abilityData)
	{
		if (RuleCalculateHitChances.TryGetHitChanceRule(caster, Game.Instance.DefaultUnit, abilityData, 0, Vector3.zero, Vector3.zero, out var rule))
		{
			return Rulebook.Trigger(rule);
		}
		return null;
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

	private static void FillWeaponStats(ItemTooltipData data, ItemEntityWeapon weapon, RuleCalculateStatsWeapon weaponStats = null)
	{
		if (weapon.Blueprint.IsRanged)
		{
			data.Texts[TooltipElement.MaxDistance] = weapon.AttackRange.ToString();
		}
		if (weaponStats == null)
		{
			weaponStats = weapon.GetWeaponStats();
		}
		int resultAdditionalHitChance = weaponStats.ResultAdditionalHitChance;
		data.Texts[TooltipElement.HitChance] = ((resultAdditionalHitChance > 0) ? UIUtilityText.AddPercentTo(resultAdditionalHitChance) : string.Empty);
		int overpenetrationChance = weapon.OverpenetrationChance;
		data.Texts[TooltipElement.OverpenetrationChance] = ((overpenetrationChance != 0) ? UIUtilityText.AddPercentTo(overpenetrationChance) : string.Empty);
		int recoil = weapon.Recoil;
		data.Texts[TooltipElement.Recoil] = ((recoil != 0) ? UIUtilityText.AddPercentTo(recoil) : string.Empty);
	}

	private static void FillWeaponAbilities(ItemTooltipData data, ItemEntityWeapon weapon, MechanicEntity casterOverride = null)
	{
		foreach (WeaponAbility weaponAbility in weapon.Blueprint.WeaponAbilities)
		{
			try
			{
				data.Abilities.Add(GetUIAbilityData(weaponAbility.Ability, weapon, casterOverride));
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

	private static string GetMechanicDescription(ItemEntity item, MechanicEntity wielderOverride = null)
	{
		return GetDescriptionWithItemEquipped(item, () => item.Description, wielderOverride);
	}

	public static string GetDescriptionWithItemEquipped(ItemEntity item, Func<string> descriptionProvider, MechanicEntity wielderOverride = null)
	{
		if (item == null)
		{
			return descriptionProvider?.Invoke() ?? string.Empty;
		}
		using (GameLogContext.Scope)
		{
			if (wielderOverride != null)
			{
				GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)wielderOverride;
				ItemEntityWeapon weapon = (item.IsDisposed ? null : (item as ItemEntityWeapon));
				AbilityData weaponPreviewAbility = GetWeaponPreviewAbility(weapon, wielderOverride);
				using (EvalContext.Build().Weapon(weapon).Ability(weaponPreviewAbility)
					.Push())
				{
					return descriptionProvider?.Invoke();
				}
			}
			if (item.Owner == null)
			{
				EnsureCalculatorUnitPair();
				ItemEntity itemEntity = item.Blueprint.CreateEntity();
				if (itemEntity == null)
				{
					return item.Description;
				}
				item.CopyRuntimeStateTo(itemEntity);
				using (ContextData<ItemSlot.IgnoreLock>.Request())
				{
					using (ContextData<GameCommandHelper.PreviewItem>.Request())
					{
						GameCommandHelper.EquipItemAutomatically(itemEntity, s_CalculatorUnitPair.CalculatorUnit);
						GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)s_CalculatorUnitPair.CurrentSelectedUnit;
						ItemEntityWeapon weapon2 = itemEntity as ItemEntityWeapon;
						AbilityData weaponPreviewAbility2 = GetWeaponPreviewAbility(weapon2, s_CalculatorUnitPair.CurrentSelectedUnit);
						using (EvalContext.Build().Weapon(weapon2).Ability(weaponPreviewAbility2)
							.Push())
						{
							return descriptionProvider?.Invoke();
						}
					}
				}
			}
			GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)item.Owner;
			ItemEntityWeapon weapon3 = item as ItemEntityWeapon;
			AbilityData weaponPreviewAbility3 = GetWeaponPreviewAbility(weapon3, item.Owner);
			using (EvalContext.Build().Weapon(weapon3).Ability(weaponPreviewAbility3)
				.Push())
			{
				return descriptionProvider?.Invoke();
			}
		}
	}

	public static string GetDescriptionWithItemEquipped(BlueprintItem blueprintItem, Func<string> descriptionProvider)
	{
		if (blueprintItem == null)
		{
			return string.Empty;
		}
		using (GameLogContext.Scope)
		{
			EnsureCalculatorUnitPair();
			ItemEntity itemEntity = blueprintItem.CreateEntity();
			if (itemEntity == null)
			{
				return blueprintItem.Description;
			}
			using (ContextData<ItemSlot.IgnoreLock>.Request())
			{
				using (ContextData<GameCommandHelper.PreviewItem>.Request())
				{
					GameCommandHelper.EquipItemAutomatically(itemEntity, s_CalculatorUnitPair.CalculatorUnit);
					GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)s_CalculatorUnitPair.CurrentSelectedUnit;
					ItemEntityWeapon weapon = itemEntity as ItemEntityWeapon;
					AbilityData weaponPreviewAbility = GetWeaponPreviewAbility(weapon, s_CalculatorUnitPair.CurrentSelectedUnit);
					using (EvalContext.Build().Weapon(weapon).Ability(weaponPreviewAbility)
						.Push())
					{
						return descriptionProvider?.Invoke();
					}
				}
			}
		}
	}

	private static string GetUsableDescription(ItemEntityUsable usable, BlueprintAbility ability, MechanicEntity wielderOverride = null)
	{
		return GetDescriptionWithItemEquipped(usable, delegate
		{
			object obj = usable.Abilities.FirstOrDefault()?.Data.ShortenedDescription;
			if (obj == null)
			{
				BlueprintAbility blueprintAbility = ability;
				if (blueprintAbility == null)
				{
					return (string)null;
				}
				obj = blueprintAbility.GetShortenedDescription();
			}
			return (string)obj;
		}, wielderOverride);
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
		if (item is ItemEntityWeapon itemEntityWeapon)
		{
			return (itemEntityWeapon.Blueprint.IsTwoHanded || itemEntityWeapon.Blueprint.IsDoubleHanded) ? UIStrings.Instance.Tooltips.TwoHanded : UIStrings.Instance.Tooltips.OneHanded;
		}
		return string.Empty;
	}

	public static string GetHandUse(BlueprintItem blueprintItem)
	{
		if (!(blueprintItem is BlueprintItemWeapon blueprintItemWeapon))
		{
			return string.Empty;
		}
		return (blueprintItemWeapon.IsTwoHanded || blueprintItemWeapon.IsDoubleHanded) ? UIStrings.Instance.Tooltips.TwoHanded : UIStrings.Instance.Tooltips.OneHanded;
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

	private static int ApplyFractionModifier(int baseValue, StatType stat, StatFactionModifierConfig[] fractionMods)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 1f;
		foreach (StatFactionModifierConfig statFactionModifierConfig in fractionMods)
		{
			if (statFactionModifierConfig.Stat == stat)
			{
				switch (statFactionModifierConfig.ModifierType)
				{
				case ModifierType.ValAdd:
					num += (float)statFactionModifierConfig.Value;
					break;
				case ModifierType.PctAdd:
					num2 += (float)statFactionModifierConfig.Value / 100f;
					break;
				case ModifierType.PctMul:
					num3 *= (float)statFactionModifierConfig.Value / 100f;
					break;
				}
			}
		}
		return Mathf.RoundToInt(((float)baseValue + num) * (1f + num2) * num3);
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
		int item = Math.Max(0, damageInfo.Modifiers.Apply(damageInfo.MinValueBase, (Modifier m) => m.Descriptor == ModifierDescriptor.Faction));
		val = Math.Max(val, damageInfo.Modifiers.Apply(damageInfo.MaxValueBase, (Modifier m) => m.Descriptor == ModifierDescriptor.Faction));
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
		return GetItemTooltipData(item, null);
	}

	public static ItemTooltipData GetItemTooltipData(ItemEntity item, MechanicEntity wielderOverride)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			if (item.IsDisposed)
			{
				return GetItemTooltipData(item.Blueprint);
			}
			ItemTooltipData itemTooltipData = new ItemTooltipData(item);
			ItemEntityUsable usable = itemTooltipData.Usable;
			itemTooltipData.Texts[TooltipElement.Name] = item.Name;
			itemTooltipData.Texts[TooltipElement.Count] = (item.IsStackable ? item.Count.ToString() : "");
			itemTooltipData.Texts[TooltipElement.ItemType] = GetItemGroup(item);
			itemTooltipData.Texts[TooltipElement.ItemCost] = UIUtilityText.GetCostFormatted(item.Blueprint.Cost);
			itemTooltipData.Texts[TooltipElement.Subname] = GetItemType(item);
			try
			{
				Dictionary<TooltipElement, string> texts = itemTooltipData.Texts;
				MechanicEntity owner = item.Owner;
				texts[TooltipElement.Price] = ((owner != null && !owner.IsDisposed) ? Game.Instance.TradeLogic.GetItemCost(item).ToString() : "0");
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
				itemTooltipData.Texts[TooltipElement.WeaponCategory] = UIStrings.Instance.WeaponCategories.GetWeaponCategoryLabel(blueprintItemWeapon.Category);
				itemTooltipData.Texts[TooltipElement.WeaponFamily] = UIStrings.Instance.WeaponCategories.GetWeaponFamilyLabel(blueprintItemWeapon.Family);
			}
			if (item.Blueprint is BlueprintItemNote || item.Blueprint is BlueprintItemKey)
			{
				if (CanReadItem(item))
				{
					if (string.IsNullOrEmpty(GetFlavorDescription(item)))
					{
						itemTooltipData.Texts[TooltipElement.ArtisticDescription] = GetMechanicDescription(item, wielderOverride);
						itemTooltipData.Texts[TooltipElement.ShortDescription] = GetMechanicDescription(item, wielderOverride);
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
				itemTooltipData.Texts[TooltipElement.ShortDescription] = GetMechanicDescription(item, wielderOverride);
			}
			FillItemStats(item, itemTooltipData, wielderOverride);
			if (item.IsIdentified && usable != null)
			{
				itemTooltipData.Texts[TooltipElement.Charges] = usable.Charges.ToString();
				itemTooltipData.Texts[TooltipElement.CasterLevel] = string.Empty;
				BlueprintAbility blueprintAbility = usable.Blueprint.Abilities.FirstOrDefault();
				if (blueprintAbility != null)
				{
					itemTooltipData.Texts[TooltipElement.Cooldown] = blueprintAbility.CooldownRounds.ToString();
					itemTooltipData.Texts[TooltipElement.Target] = blueprintAbility.GetTarget(-1, wielderOverride ?? item.Owner);
					itemTooltipData.BlueprintAbility = blueprintAbility;
					itemTooltipData.Texts[TooltipElement.ShortDescription] = GetUsableDescription(usable, blueprintAbility, wielderOverride);
					itemTooltipData.Texts[TooltipElement.LongDescription] = usable.Abilities.FirstOrDefault()?.Data.Description ?? blueprintAbility?.GetShortenedDescription();
					itemTooltipData.Texts[TooltipElement.SpellDescriptor] = string.Empty;
					itemTooltipData.Texts[TooltipElement.CastingTime] = string.Empty;
					FillEquipmentAbilities(itemTooltipData, usable.Blueprint);
					FillEquipmentDamage(itemTooltipData, usable.Blueprint);
					FillEquipmentStatsBonuses(itemTooltipData, usable.Blueprint);
				}
			}
			return itemTooltipData;
		}
	}

	public static ItemTooltipData GetItemTooltipData(ItemEntity item, bool replenishing)
	{
		return GetItemTooltipData(item, replenishing, null);
	}

	public static ItemTooltipData GetItemTooltipData(ItemEntity item, bool replenishing, MechanicEntity wielderOverride)
	{
		ItemTooltipData itemTooltipData = GetItemTooltipData(item, wielderOverride);
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
				_ = LocalizedTexts.Instance.Stats;
				itemTooltipData.Texts[TooltipElement.WeaponCategory] = UIStrings.Instance.WeaponCategories.GetWeaponCategoryLabel(blueprintItemWeapon.Category);
				itemTooltipData.Texts[TooltipElement.WeaponFamily] = UIStrings.Instance.WeaponCategories.GetWeaponFamilyLabel(blueprintItemWeapon.Family);
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

	private static void EnsureCalculatorUnitPair()
	{
		ReactiveProperty<BaseUnitEntity> selectedUnitInUI = Game.Instance.Controllers.SelectionCharacter.SelectedUnitInUI;
		if (s_CalculatorUnitPair == null)
		{
			s_CalculatorUnitPair = new CalculatorUnitPair(selectedUnitInUI);
		}
		if (s_CalculatorUnitPair.CurrentSelectedUnit == null)
		{
			s_CalculatorUnitPair.Dispose();
			s_CalculatorUnitPair = new CalculatorUnitPair(selectedUnitInUI);
		}
	}

	private static void FillItemStats(ItemEntity item, ItemTooltipData itemTooltipData, MechanicEntity wielderOverride = null)
	{
		if (!(item is ItemEntityWeapon itemEntityWeapon))
		{
			if (item is ItemEntityArmor itemEntityArmor)
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
				itemTooltipData.Texts[TooltipElement.FullArmorClass] = LocalizedTexts.Instance.Stats.GetText(itemEntityArmor.Blueprint.Category);
				if (!string.IsNullOrEmpty(armorData.ArmorDamageReduceDescription))
				{
					itemTooltipData.Texts[TooltipElement.ArmorDamageReduceDescription] = armorData.ArmorDamageReduceDescription;
				}
			}
			return;
		}
		RuleCalculateStatsWeapon weaponStats = itemEntityWeapon.GetWeaponStats();
		MechanicEntity mechanicEntity = wielderOverride ?? item.Owner ?? UtilityParty.GetCurrentSelectedUnit();
		itemTooltipData.Texts[TooltipElement.AttackType] = GetRangeType(itemEntityWeapon);
		itemTooltipData.Texts[TooltipElement.ProficiencyGroup] = GetProficiencyGroup(itemEntityWeapon);
		FillPenetration(itemTooltipData, itemEntityWeapon);
		if (itemEntityWeapon.Owner == null && wielderOverride == null)
		{
			EnsureCalculatorUnitPair();
			if (itemEntityWeapon.Blueprint.CreateEntity() is ItemEntityWeapon itemEntityWeapon2)
			{
				itemEntityWeapon.CopyRuntimeStateTo(itemEntityWeapon2);
				using (ContextData<ItemSlot.IgnoreLock>.Request())
				{
					using (ContextData<GameCommandHelper.PreviewItem>.Request())
					{
						GameCommandHelper.EquipItemAutomatically(itemEntityWeapon2, s_CalculatorUnitPair.CalculatorUnit);
						RuleCalculateStatsWeapon weaponStats2 = itemEntityWeapon2.GetWeaponStats(s_CalculatorUnitPair.CalculatorUnit);
						FillRateOfFire(itemTooltipData, itemEntityWeapon, weaponStats2);
						FillWeaponDamage(itemTooltipData, weaponStats, weaponStats2, itemEntityWeapon);
						FillWeaponStats(itemTooltipData, itemEntityWeapon, weaponStats2);
						FillWeaponAbilities(itemTooltipData, itemEntityWeapon2, s_CalculatorUnitPair.CalculatorUnit);
						return;
					}
				}
			}
		}
		RuleCalculateStatsWeapon ruleCalculateStatsWeapon = ((mechanicEntity != null) ? itemEntityWeapon.GetWeaponStats(mechanicEntity) : null);
		RuleCalculateStatsWeapon weaponStats3 = ruleCalculateStatsWeapon ?? weaponStats;
		FillRateOfFire(itemTooltipData, itemEntityWeapon, weaponStats3);
		FillWeaponDamage(itemTooltipData, weaponStats, ruleCalculateStatsWeapon, itemEntityWeapon);
		FillWeaponStats(itemTooltipData, itemEntityWeapon, weaponStats3);
		FillWeaponAbilities(itemTooltipData, itemEntityWeapon, mechanicEntity);
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
		return item.Icon.GetDefaultIfNull(DefaultImageType.Item);
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

	public static string GetTagName<T>(T tag) where T : BlueprintComponent
	{
		if (!(tag.OwnerBlueprint is BlueprintFeature { Description: var description } blueprintFeature))
		{
			return null;
		}
		ReadOnlySpan<char> span = description.AsSpan(0, Math.Min(30, description.Length));
		int num = span.IndexOf('—');
		if (num < 0)
		{
			num = span.IndexOf(':');
		}
		if (num <= 0)
		{
			return StripTags(blueprintFeature.Name);
		}
		return CapitalizeWords(StripTags(description.Substring(0, num)).ToLower()).Trim();
	}

	public static string GetTagDescription<T>(T tag) where T : BlueprintComponent
	{
		if (!(tag.OwnerBlueprint is BlueprintFeature { Description: var description }))
		{
			return null;
		}
		ReadOnlySpan<char> span = description.AsSpan(0, Math.Min(30, description.Length));
		int num = span.IndexOf('—');
		if (num < 0)
		{
			num = span.IndexOf(':');
		}
		if (num <= 0)
		{
			return description;
		}
		string text = description;
		int num2 = num + 1;
		return text.Substring(num2, text.Length - num2).Trim();
	}

	private static string CapitalizeWords(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		StringBuilder stringBuilder = new StringBuilder(input.Length);
		bool flag = true;
		foreach (char c in input)
		{
			if (char.IsWhiteSpace(c))
			{
				flag = true;
				stringBuilder.Append(c);
			}
			else if (flag)
			{
				stringBuilder.Append(char.ToUpperInvariant(c));
				flag = false;
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	private static string StripTags(string input)
	{
		StringBuilder stringBuilder = new StringBuilder(input.Length);
		bool flag = false;
		foreach (char c in input)
		{
			switch (c)
			{
			case '<':
				flag = true;
				continue;
			case '>':
				flag = false;
				continue;
			}
			if (!flag)
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public static Dictionary<SpecialWeaponDamageType, string> GetSpecialDamageValues(BlueprintItemWeapon weapon)
	{
		Dictionary<SpecialWeaponDamageType, string> dictionary = new Dictionary<SpecialWeaponDamageType, string>();
		if (weapon == null)
		{
			return dictionary;
		}
		int brutalDamage = weapon.GetBrutalDamage();
		if (brutalDamage != 0)
		{
			dictionary.Add(SpecialWeaponDamageType.Brutal, brutalDamage.ToStringWithSign());
		}
		int damageVital = weapon.GetDamageVital();
		if (damageVital != 0)
		{
			dictionary.Add(SpecialWeaponDamageType.Vital, damageVital.ToStringWithSign());
		}
		int destructiveDamage = weapon.GetDestructiveDamage();
		if (destructiveDamage != 0)
		{
			dictionary.Add(SpecialWeaponDamageType.Destructive, destructiveDamage.ToStringWithSign());
		}
		return dictionary;
	}

	public static Dictionary<SpecialWeaponDamageType, string> GetSpecialDamageValues(ItemEntityWeapon weapon)
	{
		Dictionary<SpecialWeaponDamageType, string> dictionary = new Dictionary<SpecialWeaponDamageType, string>();
		if (weapon == null)
		{
			return dictionary;
		}
		if (weapon.BrutalDamage != 0)
		{
			dictionary.Add(SpecialWeaponDamageType.Brutal, weapon.BrutalDamage.ToStringWithSign());
		}
		if (weapon.DestructiveDamage != 0)
		{
			dictionary.Add(SpecialWeaponDamageType.Destructive, weapon.DestructiveDamage.ToStringWithSign());
		}
		if (weapon.DamageVital != 0)
		{
			dictionary.Add(SpecialWeaponDamageType.Vital, weapon.DamageVital.ToStringWithSign());
		}
		return dictionary;
	}
}
