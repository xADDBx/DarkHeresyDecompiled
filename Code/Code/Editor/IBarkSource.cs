using System.Collections.Generic;
using Kingmaker.Localization;

namespace Code.Editor;

public interface IBarkSource
{
	IEnumerable<LocalizedString> Barks { get; }

	bool IsVoIdForced { get; }

	List<string> ForcedVoGuids { get; }

	bool Spammable { get; }
}
