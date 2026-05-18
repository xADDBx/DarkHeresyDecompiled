using System;
using System.Collections.Generic;
using System.Linq;
using Code.Enums;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.AreaEffects;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.EntityInfo;

public class AreaEffectInfoProvider : IEntityInfoProvider<NodeInfo>
{
	private readonly AreaEffectEventType[] m_EventTypes;

	private readonly List<IEntityInfoDescription> m_DescriptionsCached;

	private readonly UIStrings m_UIStrings;

	public AreaEffectInfoProvider(UIStrings uiStrings)
	{
		m_UIStrings = uiStrings;
		m_DescriptionsCached = new List<IEntityInfoDescription>();
		m_EventTypes = Enum.GetValues(typeof(AreaEffectEventType)).OfType<AreaEffectEventType>().ToArray();
	}

	public bool TryGetEntityInfo(NodeInfo info, out IEntityInfo entityInfo)
	{
		if (!TryGetAreaEffects(info, out var effectEntities))
		{
			entityInfo = null;
			return false;
		}
		m_DescriptionsCached.Clear();
		(string, Color?, int) tuple = (null, null, int.MinValue);
		foreach (AreaEffectEntity item in effectEntities)
		{
			AreaEffectUISettings component = item.Blueprint.GetComponent<AreaEffectUISettings>();
			ProcessTextDescription(item.Blueprint, component);
			ProcessMechanicsDescription(item.GetEffectDescription());
			string name = item.Blueprint.Name;
			if (!string.IsNullOrEmpty(name))
			{
				int num = component?.QuickInspectSettings.GetPriority() ?? int.MinValue;
				if (string.IsNullOrEmpty(tuple.Item1) || num > tuple.Item3)
				{
					tuple = (name, component?.QuickInspectSettings.GetNameColor(), num);
				}
			}
		}
		if (m_DescriptionsCached.Count < 1)
		{
			entityInfo = null;
			return false;
		}
		entityInfo = new AreaEffectEntityInfo
		{
			Name = (tuple.Item1 ?? ((string)m_UIStrings.AreaEffectInfoTexts.InfoTitle)),
			NameColor = tuple.Item2,
			Descriptions = m_DescriptionsCached,
			WorldPosition = info.Node.Vector3Position()
		};
		return true;
	}

	private bool TryGetAreaEffects(NodeInfo info, out IEnumerable<AreaEffectEntity> effectEntities)
	{
		if (!info.IsTurnBasedMode)
		{
			effectEntities = null;
			return false;
		}
		if (info.IsHighlighted)
		{
			effectEntities = info.Node.GetAreaEffects();
			return effectEntities.Any();
		}
		effectEntities = info.Node.GetAreaEffects(IsForceShow);
		return effectEntities.Any();
		static bool IsForceShow(AreaEffectEntity entity)
		{
			return entity.Blueprint.GetComponent<AreaEffectUISettings>()?.QuickInspectSettings.ForceShow ?? false;
		}
	}

	private void ProcessTextDescription(BlueprintAreaEffect blueprint, AreaEffectUISettings uiSettings)
	{
		if (!string.IsNullOrEmpty(blueprint.Description))
		{
			m_DescriptionsCached.Add(new AreaEffectInfoEntry
			{
				Icon = uiSettings?.QuickInspectSettings.GetDescriptionIcon(),
				Text = blueprint.Description,
				Color = uiSettings?.QuickInspectSettings.GetDescriptionColor()
			});
		}
	}

	private void ProcessMechanicsDescription(AreaEffectDescription effectDescription)
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
