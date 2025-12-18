using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[ComponentName("Root/NewGameRoot")]
[TypeId("0bf5b8a4f9fc49d0b846d116a0097cf3")]
public class NewGameRoot : BlueprintScriptableObject
{
	[SerializeField]
	private BlueprintAreaPresetReference m_NewGamePreset;

	[SerializeField]
	private List<NewGamePreset> m_NewGamePresetToChoose;

	[SerializeField]
	private BlueprintAreaReference _startArea;

	[SerializeField]
	private BlueprintCampaignReference[] m_StoryCampaigns;

	[SerializeField]
	private BlueprintUnitReference m_DefaultPlayerCharacter;

	public ActionList StartGameActions;

	private BlueprintCampaignReference m_MainCampaignCachedRef;

	[SerializeField]
	private bool m_SkipMainMenu;

	[SerializeField]
	public List<NewGamePreset> NewGamePresetToChoose => m_NewGamePresetToChoose;

	public IEnumerable<BlueprintCampaign> StoryCampaigns => m_StoryCampaigns?.Dereference();

	public BlueprintUnit DefaultPlayerCharacter => m_DefaultPlayerCharacter;

	public bool SkipMainMenu => m_SkipMainMenu;

	public BlueprintAreaPreset NewGamePreset
	{
		get
		{
			return m_NewGamePreset?.Get();
		}
		set
		{
			m_NewGamePreset = value.ToReference<BlueprintAreaPresetReference>();
		}
	}

	[CanBeNull]
	public BlueprintArea StartArea
	{
		get
		{
			return _startArea?.Get();
		}
		set
		{
			_startArea = value.ToReference<BlueprintAreaReference>();
		}
	}

	public BlueprintCampaign MainCampaign
	{
		get
		{
			if (m_MainCampaignCachedRef == null || m_MainCampaignCachedRef.IsEmpty())
			{
				m_MainCampaignCachedRef = StoryCampaigns.FirstOrDefault((BlueprintCampaign bp) => bp.IsMainGameContent)?.ToReference<BlueprintCampaignReference>();
			}
			if (m_MainCampaignCachedRef != null && !m_MainCampaignCachedRef.IsEmpty())
			{
				return m_MainCampaignCachedRef?.Get();
			}
			return null;
		}
	}
}
