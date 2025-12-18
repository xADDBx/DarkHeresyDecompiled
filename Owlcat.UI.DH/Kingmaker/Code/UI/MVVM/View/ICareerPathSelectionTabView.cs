namespace Kingmaker.Code.UI.MVVM.View;

public interface ICareerPathSelectionTabView
{
	void Initialize();

	bool IsTabActive();

	void UpdateState();

	void Unbind();
}
