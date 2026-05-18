using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Visual.Sound;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Framework.VO;

[TypeId("261679050d354537802d336e5d6648a0")]
public class VOSettings : BlueprintScriptableObject
{
	public class VOSettingsReference : BlueprintReference<VOSettings>
	{
		public VOSettingsReference()
		{
			guid = "0be09a66649048fb8cb2edd8c41e0d95";
		}
	}

	private static readonly VOSettingsReference s_Instance = new VOSettingsReference();

	public List<BlueprintUnitReference> CompanionBlueprints = new List<BlueprintUnitReference>();

	public BlueprintUnitReference ServoSkull;

	public BpRef<BlueprintFeature> PsykanaUserFeature;

	public BpRef<BlueprintFeature> DeamonFeature;

	public BpRef<BlueprintFeature> PariahFeature;

	[VerticalLayout]
	public MismatchMapObjects MismatchMapObjects = new MismatchMapObjects();

	public List<BlueprintDialogReference> NotUsedDialogues = new List<BlueprintDialogReference>();

	public List<AskTypeToPrototypeEntry> AskTypeToPrototypeMap = new List<AskTypeToPrototypeEntry>();

	public const string MISSING_VOGUID = "Missing VoGuid";

	private const string MISSING_VOID = "Missing VoID";

	[VerticalLayout]
	public VOCharactersMap VOCharactersMap = new VOCharactersMap();

	public static VOSettings Instance => s_Instance;

	public string GetVoGuidByBlueprintName(string unitName)
	{
		if (string.IsNullOrEmpty(unitName) || !VOCharactersMap.BlueprintUnitNameToVoGuidMap.TryGetValue(unitName, out var value))
		{
			return "Missing VoGuid for Unit " + unitName;
		}
		return value;
	}

	public string GetVoIdByBlueprint(BlueprintUnit blueprintUnit)
	{
		if (string.IsNullOrEmpty(blueprintUnit.name) || !VOCharactersMap.BlueprintUnitNameToVoGuidMap.TryGetValue(blueprintUnit.name, out var value))
		{
			return string.Format("{0} for Unit {1}", "Missing VoGuid", blueprintUnit);
		}
		return GetVoIdByGuid(value);
	}

	public string GetVoIdByGuid(string guid)
	{
		if (string.IsNullOrEmpty(guid))
		{
			return "Missing VoID for VOGUID " + guid;
		}
		if (guid.Contains("Missing VoGuid"))
		{
			return guid;
		}
		if (!VOCharactersMap.VoGuidToCharacterEntryMap.TryGetValue(guid, out var value))
		{
			return "Missing VoID for VOGUID " + guid;
		}
		return value.VoId;
	}

	[CanBeNull]
	public BlueprintUnitAsksList GetAsksByVoGuid(string ownerVoGuid)
	{
		if (VOCharactersMap.VoGuidToCharacterEntryMap.TryGetValue(ownerVoGuid, out var value))
		{
			return value.Asks;
		}
		return null;
	}
}
