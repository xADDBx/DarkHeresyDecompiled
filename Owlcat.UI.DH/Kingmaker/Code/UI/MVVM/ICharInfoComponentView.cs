namespace Kingmaker.Code.UI.MVVM;

public interface ICharInfoComponentView
{
	bool IsBinded { get; }

	void BindSection(CharInfoComponentVM vm);

	void UnbindSection();
}
