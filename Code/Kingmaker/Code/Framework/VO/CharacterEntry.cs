using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;

namespace Kingmaker.Code.Framework.VO;

[Serializable]
[VerticalLayout]
public class CharacterEntry
{
	public string Guid;

	public string VoId;

	public Gender Gender = Gender.Unknown;

	public Race Race;

	public bool ReactToPariah;

	public VoiceRaceType VoiceRaceType;

	public string Description;

	public string PathToDescriptionDoc;

	public List<BlueprintUnitReference> Units = new List<BlueprintUnitReference>();

	public string Folder = string.Empty;

	public BlueprintUnitAsksListReference Asks;
}
