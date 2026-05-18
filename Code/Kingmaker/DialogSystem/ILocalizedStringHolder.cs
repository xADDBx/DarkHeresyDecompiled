using Kingmaker.Localization;

namespace Kingmaker.DialogSystem;

public interface ILocalizedStringHolder
{
	LocalizedString LocalizedStringText { get; }
}
