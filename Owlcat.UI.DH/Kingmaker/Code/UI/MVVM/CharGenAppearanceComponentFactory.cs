using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Customization;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.Visual.CharacterSystem;
using ObservableCollections;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public static class CharGenAppearanceComponentFactory
{
	public static BaseCharGenAppearancePageComponentVM GetComponent(CharGenAppearancePageComponent componentType, CharGenContext ctx)
	{
		return componentType switch
		{
			CharGenAppearancePageComponent.Gender => GetGenderSelectorVM(ctx), 
			CharGenAppearancePageComponent.FaceType => GetFaceSelectorVM(ctx), 
			CharGenAppearancePageComponent.BodyType => GetBodySelectorVM(ctx), 
			CharGenAppearancePageComponent.SkinColour => GetBodyColorSelectorVM(ctx), 
			CharGenAppearancePageComponent.HairType => GetHairSelectorVM(ctx), 
			CharGenAppearancePageComponent.HairColour => GetHairColorSelectorVM(ctx), 
			CharGenAppearancePageComponent.EyebrowType => GetEyebrowsSelectorVM(ctx), 
			CharGenAppearancePageComponent.EyebrowColour => GetEyebrowsColorSelectorVM(ctx), 
			CharGenAppearancePageComponent.BeardType => GetBeardSelectorVM(ctx), 
			CharGenAppearancePageComponent.BeardColour => GetBeardColorSelectorVM(ctx), 
			CharGenAppearancePageComponent.ScarsType => GetScarsSelectorVM(ctx), 
			CharGenAppearancePageComponent.Tattoo => GetTattooAndColorSelectorVM(ctx), 
			CharGenAppearancePageComponent.TattooColor => GetTattooColorSelector(ctx), 
			CharGenAppearancePageComponent.PortType1 => GetPortsSelectorVM(ctx, 0), 
			CharGenAppearancePageComponent.PortType2 => GetPortsSelectorVM(ctx, 1), 
			CharGenAppearancePageComponent.Augmentic => GetImplantsSelectorVM(ctx), 
			CharGenAppearancePageComponent.VoiceType => GetVoiceSelectorVM(ctx), 
			CharGenAppearancePageComponent.ServoSkullType => GetServoSkullSelectorVM(ctx), 
			_ => null, 
		};
	}

	public static void UpdateComponent(CharGenAppearancePageComponent componentType, VirtualListElementVMBase component, CharGenContext ctx)
	{
		switch (componentType)
		{
		case CharGenAppearancePageComponent.Gender:
			UpdateGenderSelectorVM(ctx, component as TextureSequentialSelectorVM);
			break;
		case CharGenAppearancePageComponent.FaceType:
			UpdateFaceSelectorVM(ctx, component as TextureSelectorVM);
			break;
		case CharGenAppearancePageComponent.BodyType:
			UpdateBodySelectorVM(ctx, component as SlideSequentialSelectorVM);
			break;
		case CharGenAppearancePageComponent.SkinColour:
			UpdateBodyColorList(ctx, component as TextureSelectorVM);
			break;
		case CharGenAppearancePageComponent.HairType:
			UpdateHairList(ctx, component as TextureSelectorVM);
			break;
		case CharGenAppearancePageComponent.HairColour:
			UpdateHairColorList(ctx, component as TextureSelectorVM);
			break;
		case CharGenAppearancePageComponent.EyebrowType:
			UpdateEyebrowsList(ctx, component as TextureSelectorVM);
			break;
		case CharGenAppearancePageComponent.EyebrowColour:
			UpdateEyebrowColorList(ctx, component as TextureSelectorVM);
			break;
		case CharGenAppearancePageComponent.BeardType:
			UpdateBeardList(ctx, component as TextureSelectorVM);
			break;
		case CharGenAppearancePageComponent.BeardColour:
			UpdateBeardColorList(ctx, component as TextureSelectorVM);
			break;
		case CharGenAppearancePageComponent.ScarsType:
			UpdateScarsSelectorVM(ctx, component as SlideSequentialSelectorVM);
			break;
		case CharGenAppearancePageComponent.Tattoo:
			UpdateTattooAndColorSelector(ctx, component as TextureAndColorGroupSelectorVM);
			break;
		case CharGenAppearancePageComponent.TattooColor:
			UpdateTattooColorList(ctx, ctx.CurrentTattooSet, component as TextureSelectorVM);
			break;
		case CharGenAppearancePageComponent.PortType1:
			UpdatePortsList(ctx, component as TextureSelectorVM, 0);
			break;
		case CharGenAppearancePageComponent.PortType2:
			UpdatePortsList(ctx, component as TextureSelectorVM, 1);
			break;
		case CharGenAppearancePageComponent.Augmentic:
			UpdatePortsAndColorSelector(ctx, component as TextureAndColorGroupSelectorVM);
			break;
		case CharGenAppearancePageComponent.VoiceType:
		case CharGenAppearancePageComponent.ServoSkullType:
			break;
		}
	}

	private static CharGenVoiceSelectorVM GetVoiceSelectorVM(CharGenContext ctx)
	{
		return new CharGenVoiceSelectorVM(ctx)
		{
			Type = CharGenAppearancePageComponent.VoiceType
		};
	}

	public static void GetTextureSelectorItemVM(ObservableList<TextureSelectorItemVM> valueList, int i, Texture2D item, Action setter, bool isEmpty = false)
	{
		if (valueList.Count > i)
		{
			valueList[i].UpdateTextureAndSetter(item, setter, isEmpty);
		}
		else
		{
			valueList.Add(new TextureSelectorItemVM(item, setter, i, allowSwitchOff: false, null, isEmpty));
		}
	}

	private static void UpdateAppearanceList(CharGenAppearancePageComponent componentType, CharGenContext ctx, TextureSelectorVM selector, Func<CustomizationOptions, EquipmentEntityLink[]> listSelector, Func<DollState, DollState.EEAdapter> currentSelector, Action<EquipmentEntityLink, int> setter, bool isOptional = false)
	{
		UpdateAppearanceListImpl(componentType, ctx, selector, listSelector, (DollState s) => currentSelector(s).Load(), setter, isOptional, null);
	}

	private static void UpdateAppearanceListIndexed(CharGenAppearancePageComponent componentType, CharGenContext ctx, TextureSelectorVM selector, int slot, Func<CustomizationOptions, EquipmentEntityLink[]> listSelector, Func<DollState, List<DollState.DollPrint>> slotsSelector, Action<EquipmentEntityLink, int, int> setter, bool isOptional = false)
	{
		List<DollState.DollPrint> slots = slotsSelector(ctx.Doll);
		if (slots.Count != 0 && slot <= slots.Count)
		{
			UpdateAppearanceListImpl(componentType, ctx, selector, listSelector, (DollState _) => slots[slot].PaintEE.Load(), delegate(EquipmentEntityLink item, int i)
			{
				setter(item, i, slot);
			}, isOptional, slot);
		}
	}

	private static void UpdateAppearanceListImpl(CharGenAppearancePageComponent componentType, CharGenContext ctx, TextureSelectorVM selector, Func<CustomizationOptions, EquipmentEntityLink[]> listSelector, Func<DollState, EquipmentEntity> currentLoaded, Action<EquipmentEntityLink, int> setter, bool isOptional, int? logSlot)
	{
		DollState doll = ctx.Doll;
		if (doll.Race == null)
		{
			return;
		}
		BlueprintRaceAppearance appearance = doll.Race.Appearance;
		if (appearance == null)
		{
			return;
		}
		ObservableList<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		CustomizationOptions arg = ((doll.Gender == Gender.Male) ? appearance.MaleOptions : appearance.FemaleOptions);
		EquipmentEntityLink[] array = listSelector(arg);
		EquipmentEntity equipmentEntity = currentLoaded(doll);
		int num = 0;
		if (isOptional)
		{
			GetTextureSelectorItemVM(entitiesCollection, num, null, delegate
			{
				setter(new EquipmentEntityLink
				{
					AssetId = string.Empty
				}, -1);
			}, isEmpty: true);
			if (equipmentEntity == null)
			{
				selector.SelectionGroup.TrySelectEntity(entitiesCollection[num]);
			}
			num++;
		}
		for (int j = 0; j < array.Length; j++)
		{
			EquipmentEntityLink item = array[j];
			EquipmentEntity equipmentEntity2 = item.Load();
			if (equipmentEntity2 == null)
			{
				string arg2 = (logSlot.HasValue ? $" slot={logSlot.Value}" : string.Empty);
				PFLog.UI.Error($"[CharGenAppearance] {componentType}[{j}] failed to load (AssetId='{item?.AssetId}') — " + $"race='{doll.Race?.name}' gender={doll.Gender}{arg2}. Item skipped.");
				continue;
			}
			int i1 = j;
			GetTextureSelectorItemVM(entitiesCollection, num, equipmentEntity2.PreviewTexture, delegate
			{
				setter(item, i1);
			});
			if (equipmentEntity2 == equipmentEntity)
			{
				selector.SelectionGroup.TrySelectEntity(entitiesCollection[num]);
			}
			num++;
		}
		selector.SelectionGroup.ClearFromIndex(num);
	}

	private static TextureSequentialSelectorVM GetGenderSelectorVM(CharGenContext ctx)
	{
		TextureSequentialEntity current;
		TextureSequentialSelectorVM textureSequentialSelectorVM = new TextureSequentialSelectorVM(GetGenderList(ctx, out current), current);
		textureSequentialSelectorVM.Type = CharGenAppearancePageComponent.Gender;
		textureSequentialSelectorVM.SetTitle(UIStrings.Instance.CharGen.BodyType);
		return textureSequentialSelectorVM;
	}

	private static void UpdateGenderSelectorVM(CharGenContext ctx, TextureSequentialSelectorVM selectorVM)
	{
		TextureSequentialEntity current;
		List<TextureSequentialEntity> genderList = GetGenderList(ctx, out current);
		selectorVM.SetValues(genderList, current);
	}

	private static List<TextureSequentialEntity> GetGenderList(CharGenContext ctx, out TextureSequentialEntity current)
	{
		List<TextureSequentialEntity> list = new List<TextureSequentialEntity>();
		current = null;
		foreach (Gender gender in new List<Gender>
		{
			Gender.Male,
			Gender.Female
		})
		{
			if (gender != Gender.Unknown)
			{
				Sprite genderIcon = UIConfig.Instance.UIIcons.GetGenderIcon(gender);
				TextureSequentialEntity textureSequentialEntity = new TextureSequentialEntity
				{
					Texture = genderIcon,
					Setter = delegate
					{
						ctx.RequestSetGender(gender, (int)gender);
					}
				};
				list.Add(textureSequentialEntity);
				if (ctx.Doll.Gender == gender)
				{
					current = textureSequentialEntity;
				}
			}
		}
		return list;
	}

	private static TextureSelectorVM GetFaceSelectorVM(CharGenContext ctx)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ObservableList<TextureSelectorItemVM>()), TextureSelectorType.Default);
		UpdateFaceSelectorVM(ctx, textureSelectorVM);
		textureSelectorVM.Type = CharGenAppearancePageComponent.FaceType;
		textureSelectorVM.SetTitle(UIStrings.Instance.CharGen.Face);
		return textureSelectorVM;
	}

	private static void UpdateFaceSelectorVM(CharGenContext ctx, TextureSelectorVM selector)
	{
		UpdateAppearanceList(CharGenAppearancePageComponent.FaceType, ctx, selector, (CustomizationOptions o) => o.Head, (DollState s) => s.Head, ctx.RequestSetHead);
	}

	private static SlideSequentialSelectorVM GetBodySelectorVM(CharGenContext ctx)
	{
		StringSequentialEntity current;
		SlideSequentialSelectorVM slideSequentialSelectorVM = new SlideSequentialSelectorVM(GetBodyList(ctx, out current), current);
		slideSequentialSelectorVM.Type = CharGenAppearancePageComponent.BodyType;
		slideSequentialSelectorVM.SetTitle(UIStrings.Instance.CharGen.BodyConstitution);
		return slideSequentialSelectorVM;
	}

	private static void UpdateBodySelectorVM(CharGenContext ctx, SlideSequentialSelectorVM selectorVM)
	{
		StringSequentialEntity current;
		List<StringSequentialEntity> bodyList = GetBodyList(ctx, out current);
		selectorVM.SetValues(bodyList, current);
	}

	private static List<StringSequentialEntity> GetBodyList(CharGenContext ctx, out StringSequentialEntity current)
	{
		List<StringSequentialEntity> list = new List<StringSequentialEntity>();
		current = null;
		BlueprintRaceAppearance blueprintRaceAppearance = ctx.Doll.Race?.Appearance;
		if (blueprintRaceAppearance == null)
		{
			return list;
		}
		for (int j = 0; j < blueprintRaceAppearance.Presets.Length; j++)
		{
			BlueprintRaceVisualPreset racePreset = blueprintRaceAppearance.Presets[j];
			int i1 = j;
			StringSequentialEntity stringSequentialEntity = new StringSequentialEntity
			{
				Setter = delegate
				{
					ctx.RequestSetRace(racePreset, i1);
				}
			};
			list.Add(stringSequentialEntity);
			if (racePreset == ctx.Doll.RacePreset)
			{
				current = stringSequentialEntity;
			}
		}
		return list;
	}

	private static TextureSelectorVM GetBodyColorSelectorVM(CharGenContext ctx)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ObservableList<TextureSelectorItemVM>()), TextureSelectorType.Color);
		UpdateBodyColorList(ctx, textureSelectorVM);
		textureSelectorVM.Type = CharGenAppearancePageComponent.SkinColour;
		textureSelectorVM.SetTitle(UIStrings.Instance.CharGen.SkinTone);
		return textureSelectorVM;
	}

	private static void UpdateBodyColorList(CharGenContext ctx, TextureSelectorVM selector)
	{
		DollState doll = ctx.Doll;
		ObservableList<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		List<Texture2D> skinRamps = doll.GetSkinRamps();
		for (int j = 0; j < skinRamps.Count; j++)
		{
			int i1 = j;
			Texture2D item = skinRamps[j];
			GetTextureSelectorItemVM(entitiesCollection, j, item, delegate
			{
				ctx.RequestSetSkinColor(i1);
			});
			if (doll.SkinRampIndex == j)
			{
				selector.SelectionGroup.TrySelectEntity(entitiesCollection[j]);
			}
		}
		selector.SelectionGroup.ClearFromIndex(skinRamps.Count);
	}

	private static TextureSelectorVM GetHairSelectorVM(CharGenContext ctx)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ObservableList<TextureSelectorItemVM>()), TextureSelectorType.Default);
		UpdateHairList(ctx, textureSelectorVM);
		textureSelectorVM.Type = CharGenAppearancePageComponent.HairType;
		textureSelectorVM.SetTitle(UIStrings.Instance.CharGen.HairStyle);
		return textureSelectorVM;
	}

	private static void UpdateHairList(CharGenContext ctx, TextureSelectorVM selector)
	{
		UpdateAppearanceList(CharGenAppearancePageComponent.HairType, ctx, selector, (CustomizationOptions o) => o.Hair, (DollState s) => s.Hair, ctx.RequestSetHair, isOptional: true);
	}

	private static TextureSelectorVM GetHairColorSelectorVM(CharGenContext ctx)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ObservableList<TextureSelectorItemVM>()), TextureSelectorType.Color);
		UpdateHairColorList(ctx, textureSelectorVM);
		textureSelectorVM.Type = CharGenAppearancePageComponent.HairColour;
		textureSelectorVM.SetTitle(UIStrings.Instance.CharGen.HairColor);
		return textureSelectorVM;
	}

	private static void UpdateHairColorList(CharGenContext ctx, TextureSelectorVM selector)
	{
		DollState doll = ctx.Doll;
		ObservableList<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		List<Texture2D> hairRamps = doll.GetHairRamps();
		for (int j = 0; j < hairRamps.Count; j++)
		{
			int i1 = j;
			Texture2D item = hairRamps[j];
			GetTextureSelectorItemVM(entitiesCollection, j, item, delegate
			{
				ctx.RequestSetHairColor(i1);
			});
			if (doll.HairRampIndex == j)
			{
				selector.SelectionGroup.TrySelectEntity(entitiesCollection[j]);
			}
		}
		selector.SelectionGroup.ClearFromIndex(hairRamps.Count);
	}

	private static TextureSelectorVM GetEyebrowsSelectorVM(CharGenContext ctx)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ObservableList<TextureSelectorItemVM>()), TextureSelectorType.Default);
		UpdateEyebrowsList(ctx, textureSelectorVM);
		textureSelectorVM.Type = CharGenAppearancePageComponent.EyebrowType;
		textureSelectorVM.SetTitle(UIStrings.Instance.CharGen.Eyebrows);
		return textureSelectorVM;
	}

	private static void UpdateEyebrowsList(CharGenContext ctx, TextureSelectorVM selector)
	{
		UpdateAppearanceList(CharGenAppearancePageComponent.EyebrowType, ctx, selector, (CustomizationOptions o) => o.Eyebrows, (DollState s) => s.Eyebrows, ctx.RequestSetEyebrows);
	}

	private static TextureSelectorVM GetEyebrowsColorSelectorVM(CharGenContext ctx)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ObservableList<TextureSelectorItemVM>()), TextureSelectorType.Color);
		UpdateEyebrowColorList(ctx, textureSelectorVM);
		textureSelectorVM.Type = CharGenAppearancePageComponent.EyebrowColour;
		textureSelectorVM.SetTitle(UIStrings.Instance.CharGen.EyebrowsColor);
		return textureSelectorVM;
	}

	private static void UpdateEyebrowColorList(CharGenContext ctx, TextureSelectorVM selector)
	{
		DollState doll = ctx.Doll;
		ObservableList<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		List<Texture2D> eyebrowsRamps = doll.GetEyebrowsRamps();
		for (int j = 0; j < eyebrowsRamps.Count; j++)
		{
			int i1 = j;
			Texture2D item = eyebrowsRamps[j];
			GetTextureSelectorItemVM(entitiesCollection, j, item, delegate
			{
				ctx.RequestSetEyebrowsColor(i1);
			});
			if (doll.EyebrowsColorRampIndex == j)
			{
				selector.SelectionGroup.TrySelectEntity(entitiesCollection[j]);
			}
		}
		selector.SelectionGroup.ClearFromIndex(eyebrowsRamps.Count);
	}

	private static TextureSelectorVM GetBeardSelectorVM(CharGenContext ctx)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ObservableList<TextureSelectorItemVM>()), TextureSelectorType.Default);
		UpdateBeardList(ctx, textureSelectorVM);
		textureSelectorVM.Type = CharGenAppearancePageComponent.BeardType;
		textureSelectorVM.SetTitle(UIStrings.Instance.CharGen.Beard);
		return textureSelectorVM;
	}

	private static void UpdateBeardList(CharGenContext ctx, TextureSelectorVM selector)
	{
		UpdateAppearanceList(CharGenAppearancePageComponent.BeardType, ctx, selector, (CustomizationOptions o) => o.Facial, (DollState s) => s.Beard, ctx.RequestSetBeard, isOptional: true);
	}

	private static TextureSelectorVM GetBeardColorSelectorVM(CharGenContext ctx)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ObservableList<TextureSelectorItemVM>()), TextureSelectorType.Color);
		UpdateBeardColorList(ctx, textureSelectorVM);
		textureSelectorVM.Type = CharGenAppearancePageComponent.BeardColour;
		textureSelectorVM.SetTitle(UIStrings.Instance.CharGen.BeardColor);
		return textureSelectorVM;
	}

	private static void UpdateBeardColorList(CharGenContext ctx, TextureSelectorVM selector)
	{
		DollState doll = ctx.Doll;
		ObservableList<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		List<Texture2D> beardRamps = doll.GetBeardRamps();
		for (int j = 0; j < beardRamps.Count; j++)
		{
			int i1 = j;
			Texture2D item = beardRamps[j];
			GetTextureSelectorItemVM(entitiesCollection, j, item, delegate
			{
				ctx.RequestSetBeardColor(i1);
			});
			if (doll.BeardColorRampIndex == j)
			{
				selector.SelectionGroup.TrySelectEntity(entitiesCollection[j]);
			}
		}
		selector.SelectionGroup.ClearFromIndex(beardRamps.Count);
	}

	private static SlideSequentialSelectorVM GetScarsSelectorVM(CharGenContext ctx)
	{
		StringSequentialEntity current;
		SlideSequentialSelectorVM slideSequentialSelectorVM = new SlideSequentialSelectorVM(GetScarsList(ctx, out current), current);
		slideSequentialSelectorVM.Type = CharGenAppearancePageComponent.ScarsType;
		slideSequentialSelectorVM.SetTitle(UIStrings.Instance.CharGen.Scars);
		return slideSequentialSelectorVM;
	}

	private static void UpdateScarsSelectorVM(CharGenContext ctx, SlideSequentialSelectorVM selectorVM)
	{
		StringSequentialEntity current;
		List<StringSequentialEntity> scarsList = GetScarsList(ctx, out current);
		selectorVM.SetValues(scarsList, current);
	}

	private static List<StringSequentialEntity> GetScarsList(CharGenContext ctx, out StringSequentialEntity current)
	{
		DollState doll = ctx.Doll;
		List<StringSequentialEntity> list = new List<StringSequentialEntity>();
		current = null;
		StringSequentialEntity stringSequentialEntity = new StringSequentialEntity
		{
			Title = UIStrings.Instance.CharGen.NothingToChoose,
			Setter = delegate
			{
				ctx.RequestSetScar(new EquipmentEntityLink
				{
					AssetId = string.Empty
				}, -1);
			}
		};
		list.Add(stringSequentialEntity);
		if (doll.Scar.Load() == null)
		{
			current = stringSequentialEntity;
		}
		for (int j = 0; j < doll.Scars.Count; j++)
		{
			EquipmentEntityLink scar = doll.Scars[j];
			int i1 = j;
			StringSequentialEntity stringSequentialEntity2 = new StringSequentialEntity
			{
				Setter = delegate
				{
					ctx.RequestSetScar(scar, i1);
				}
			};
			list.Add(stringSequentialEntity2);
			if (scar.Load() == doll.Scar.Load())
			{
				current = stringSequentialEntity2;
			}
		}
		return list;
	}

	private static TextureAndColorGroupSelectorVM GetTattooAndColorSelectorVM(CharGenContext ctx)
	{
		TextureAndColorGroupSelectorVM textureAndColorGroupSelectorVM = new TextureAndColorGroupSelectorVM(ctx);
		textureAndColorGroupSelectorVM.Type = CharGenAppearancePageComponent.Tattoo;
		UpdateTattooAndColorSelector(ctx, textureAndColorGroupSelectorVM);
		return textureAndColorGroupSelectorVM;
	}

	private static void UpdateTattooAndColorSelector(CharGenContext ctx, TextureAndColorGroupSelectorVM selector)
	{
		IEnumerable<TextureSelectorVM> tattooSelectors = GetTattooSelectors(ctx);
		IEnumerable<TextureSelectorVM> tattooColorSelectors = GetTattooColorSelectors(ctx);
		selector.SetValues(tattooSelectors, tattooColorSelectors);
	}

	private static IEnumerable<TextureSelectorVM> GetTattooSelectors(CharGenContext ctx)
	{
		List<TextureSelectorVM> list = new List<TextureSelectorVM>();
		for (int i = 0; i < 5; i++)
		{
			TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ObservableList<TextureSelectorItemVM>()), TextureSelectorType.Default, hideIfNoElements: true, i);
			textureSelectorVM.SetTitle(string.Concat(UIStrings.Instance.CharGen.Tattoo, " ", i.ToString()));
			UpdateTattoosList(ctx, textureSelectorVM, i);
			list.Add(textureSelectorVM);
		}
		return list;
	}

	private static void UpdateTattoosList(CharGenContext ctx, TextureSelectorVM selector, int slot)
	{
		UpdateAppearanceListIndexed(CharGenAppearancePageComponent.Tattoo, ctx, selector, slot, (CustomizationOptions o) => o.Tatoo, (DollState s) => s.Tattoos, ctx.RequestSetTattoo, isOptional: true);
	}

	private static TextureSelectorVM GetTattooColorSelector(CharGenContext ctx)
	{
		return GetTattooColorSelector(ctx, ctx.CurrentTattooSet);
	}

	private static IEnumerable<TextureSelectorVM> GetTattooColorSelectors(CharGenContext ctx)
	{
		List<TextureSelectorVM> list = new List<TextureSelectorVM>();
		for (int i = 0; i < 5; i++)
		{
			list.Add(GetTattooColorSelector(ctx, i));
		}
		return list;
	}

	private static TextureSelectorVM GetTattooColorSelector(CharGenContext ctx, int index)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ObservableList<TextureSelectorItemVM>()), TextureSelectorType.Color, hideIfNoElements: false, index);
		textureSelectorVM.Type = CharGenAppearancePageComponent.TattooColor;
		UpdateTattooColorList(ctx, index, textureSelectorVM);
		textureSelectorVM.SetTitle(string.Concat(UIStrings.Instance.CharGen.TattooColor, " ", index.ToString()));
		textureSelectorVM.SetNoItemsDescription(UIStrings.Instance.CharGen.NothingToChoose);
		return textureSelectorVM;
	}

	public static void RefreshTattooColorSelector(CharGenContext ctx, int slot, TextureSelectorVM selector)
	{
		UpdateTattooColorList(ctx, slot, selector);
	}

	private static void UpdateTattooColorList(CharGenContext ctx, int index, TextureSelectorVM selector)
	{
		DollState doll = ctx.Doll;
		ObservableList<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		List<Texture2D> tattooRamps = doll.GetTattooRamps(index);
		for (int j = 0; j < tattooRamps.Count; j++)
		{
			int i1 = j;
			Texture2D item = tattooRamps[j];
			GetTextureSelectorItemVM(entitiesCollection, j, item, delegate
			{
				ctx.RequestSetTattooColor(i1, index);
			});
			if (doll.Tattoos[index].PaintRampIndex == j)
			{
				selector.SelectionGroup.TrySelectEntity(entitiesCollection[j]);
			}
		}
		selector.SelectionGroup.ClearFromIndex(tattooRamps.Count);
	}

	private static TextureAndColorGroupSelectorVM GetImplantsSelectorVM(CharGenContext ctx)
	{
		TextureAndColorGroupSelectorVM textureAndColorGroupSelectorVM = new TextureAndColorGroupSelectorVM(ctx);
		textureAndColorGroupSelectorVM.Type = CharGenAppearancePageComponent.Augmentic;
		UpdatePortsAndColorSelector(ctx, textureAndColorGroupSelectorVM);
		return textureAndColorGroupSelectorVM;
	}

	private static void UpdatePortsAndColorSelector(CharGenContext ctx, TextureAndColorGroupSelectorVM selector)
	{
		IEnumerable<TextureSelectorVM> portsSelectors = GetPortsSelectors(ctx);
		IEnumerable<TextureSelectorVM> colors = Enumerable.Empty<TextureSelectorVM>();
		selector.SetValues(portsSelectors, colors);
	}

	private static IEnumerable<TextureSelectorVM> GetPortsSelectors(CharGenContext ctx)
	{
		List<TextureSelectorVM> list = new List<TextureSelectorVM>();
		for (int i = 0; i < 2; i++)
		{
			list.Add(GetPortsSelectorVM(ctx, i));
		}
		return list;
	}

	private static TextureSelectorVM GetPortsSelectorVM(CharGenContext ctx, int index)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ObservableList<TextureSelectorItemVM>()), TextureSelectorType.Default);
		UpdatePortsList(ctx, textureSelectorVM, index);
		textureSelectorVM.Type = ((index == 0) ? CharGenAppearancePageComponent.PortType1 : CharGenAppearancePageComponent.PortType2);
		textureSelectorVM.SetTitle(string.Format(UIStrings.Instance.CharGen.Implant.Text, (index + 1).ToString()));
		return textureSelectorVM;
	}

	private static void UpdatePortsList(CharGenContext ctx, TextureSelectorVM selector, int slot)
	{
		UpdateAppearanceListIndexed((slot == 0) ? CharGenAppearancePageComponent.PortType1 : CharGenAppearancePageComponent.PortType2, ctx, selector, slot, (CustomizationOptions o) => o.Augmentic, (DollState s) => s.Ports, ctx.RequestSetPort, isOptional: true);
	}

	private static TextureSelectorVM GetServoSkullSelectorVM(CharGenContext ctx)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ObservableList<TextureSelectorItemVM>()), TextureSelectorType.Default);
		UpdateServoSkullList(ctx, textureSelectorVM);
		textureSelectorVM.Type = CharGenAppearancePageComponent.ServoSkullType;
		textureSelectorVM.SetTitle("ServoSkull");
		textureSelectorVM.SetDescription("ServoSkull description");
		return textureSelectorVM;
	}

	private static void UpdateServoSkullList(CharGenContext ctx, TextureSelectorVM selector)
	{
		DollState doll = ctx.Doll;
		ObservableList<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		List<Texture2D> hairRamps = doll.GetHairRamps();
		int num = 0;
		for (int j = 0; j < hairRamps.Count; j++)
		{
			int i1 = j;
			Texture2D item = hairRamps[j];
			GetTextureSelectorItemVM(entitiesCollection, num, item, delegate
			{
				PFLog.UI.Log($"Set ServoSkull: {i1}");
			});
			entitiesCollection[num].IsSelected.Value = doll.HairRampIndex == j;
			num++;
		}
		selector.SelectionGroup.ClearFromIndex(hairRamps.Count);
	}
}
