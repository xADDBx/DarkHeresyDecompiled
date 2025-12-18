using System;
using System.Collections.Generic;
using System.Linq;
using Code.Enums;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.AreaEffects;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.DotNetExtensions;
using Pathfinding;

namespace Kingmaker.Code.UI.MVVM.EntityInfo;

public class AreaEffectInfoProvider : IEntityInfoProvider<GridNodeBase>
{
	private readonly AreaEffectEventType[] m_EventTypes;

	private readonly List<IEntityInfoDescription> m_DescriptionsCached;

	private readonly UIStrings m_UIStrings;

	private readonly UIIcons m_UIIcons;

	public AreaEffectInfoProvider(UIStrings uiStrings, UIIcons uiIcons)
	{
		m_UIStrings = uiStrings;
		m_UIIcons = uiIcons;
		m_DescriptionsCached = new List<IEntityInfoDescription>();
		m_EventTypes = Enum.GetValues(typeof(AreaEffectEventType)).OfType<AreaEffectEventType>().ToArray();
	}

	public bool TryGetEntityInfo(GridNodeBase node, out IEntityInfo entityInfo)
	{
		if (node == null)
		{
			entityInfo = null;
			return false;
		}
		FilteredList<AreaEffectEntity> areaEffects = node.GetAreaEffects();
		m_DescriptionsCached.Clear();
		foreach (AreaEffectEntity item in areaEffects)
		{
			ProcessEffectDescription(item.GetEffectDescription());
		}
		if (m_DescriptionsCached.Count < 1)
		{
			entityInfo = null;
			return false;
		}
		entityInfo = new AreaEffectEntityInfo
		{
			Descriptions = m_DescriptionsCached,
			WorldPosition = node.Vector3Position()
		};
		return true;
	}

	private void ProcessEffectDescription(AreaEffectDescription effectDescription)
	{
		AreaEffectEventType[] eventTypes = m_EventTypes;
		foreach (AreaEffectEventType areaEffectEventType in eventTypes)
		{
			AreaEffectDescriptionEntry[] effect = effectDescription.GetEffect(areaEffectEventType);
			if (effect.Length >= 1)
			{
				AreaEffectDescriptionEntry[] array = effect;
				foreach (AreaEffectDescriptionEntry description in array)
				{
					ProcessEventType(areaEffectEventType, description);
				}
			}
		}
	}

	private void ProcessEventType(AreaEffectEventType eventType, AreaEffectDescriptionEntry description)
	{
		string text = m_UIStrings.AreaEffectInfoTexts.GetEventLocalizedString(eventType).Text;
		if (text != null)
		{
			if (description.Damage != null)
			{
				AreaEffectInfoEntry areaEffectInfoEntry = default(AreaEffectInfoEntry);
				areaEffectInfoEntry.Text = GetDamageText(description.Damage, text);
				AreaEffectInfoEntry areaEffectInfoEntry2 = areaEffectInfoEntry;
				m_DescriptionsCached.Add(areaEffectInfoEntry2);
			}
			BlueprintBuff buff = description.Buff;
			if (buff != null && !buff.IsHiddenInUI)
			{
				AreaEffectInfoEntry areaEffectInfoEntry = default(AreaEffectInfoEntry);
				areaEffectInfoEntry.Text = GetBuffText(description.Buff, text);
				areaEffectInfoEntry.Icon = description.Buff.Icon;
				AreaEffectInfoEntry areaEffectInfoEntry3 = areaEffectInfoEntry;
				m_DescriptionsCached.Add(areaEffectInfoEntry3);
			}
			if (description.DOT.HasValue)
			{
				DOTEffectInfoEntry dOTEffectInfoEntry = default(DOTEffectInfoEntry);
				dOTEffectInfoEntry.Text = AddDOTText(description.DOT.Value, text) ?? string.Empty;
				dOTEffectInfoEntry.Type = description.DOT.Value;
				DOTEffectInfoEntry dOTEffectInfoEntry2 = dOTEffectInfoEntry;
				m_DescriptionsCached.Add(dOTEffectInfoEntry2);
			}
		}
	}

	private string GetDamageText(ContextActionDealDamage damage, string eventString)
	{
		GameLogContext.DamageType = UtilityText.GetDamageTypeText(damage.DamageType.Type);
		GameLogContext.Description = eventString;
		return m_UIStrings.AreaEffectInfoTexts.DamageDescription.Text;
	}

	private string GetBuffText(BlueprintBuff buff, string eventString)
	{
		GameLogContext.Text = buff.LocalizedName.Text;
		GameLogContext.Description = eventString;
		return m_UIStrings.AreaEffectInfoTexts.BuffDescription.Text;
	}

	private string AddDOTText(DOT dotType, string eventString)
	{
		LocalizedString dOTLocalizedString = m_UIStrings.AreaEffectInfoTexts.GetDOTLocalizedString(dotType);
		if (dOTLocalizedString == null)
		{
			return null;
		}
		GameLogContext.Text = dOTLocalizedString.Text;
		GameLogContext.Description = eventString;
		return m_UIStrings.AreaEffectInfoTexts.DOTDescription.Text;
	}
}
