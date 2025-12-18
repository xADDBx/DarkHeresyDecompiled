using System.Collections.Generic;
using Code.Editor;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using Kingmaker.View.MapObjects.InteractionComponentBase;

namespace Kingmaker.View.MapObjects;

[KnowledgeDatabaseID("8e21ca2f1fb74fe4da1ce73a29d81212")]
public class InteractionBark : InteractionComponent<InteractionBarkPart, InteractionBarkSettings>, IBarkSource
{
	public IEnumerable<LocalizedString> Barks => Settings.Barks;

	public bool IsVoIdForced => Settings.IsVoIdForced;

	public List<string> ForcedVoGuids => Settings.ForcedVoGuids;

	public bool Spammable => Settings.Spammable;
}
