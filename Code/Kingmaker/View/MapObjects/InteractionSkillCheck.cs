using System.Collections.Generic;
using Code.Editor;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[DisallowMultipleComponent]
[KnowledgeDatabaseID("b68623916ce528f41a6c12212cecee86")]
public class InteractionSkillCheck : InteractionComponent<InteractionSkillCheckPart, InteractionSkillCheckSettings>, IBarkSource
{
	public IEnumerable<LocalizedString> Barks => Settings.Barks;

	public bool IsVoIdForced => Settings.IsVoIdForced;

	public List<string> ForcedVoGuids => Settings.ForcedVoGuids;

	public bool Spammable => Settings.Spammable;
}
