using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public interface IFuncAdditionalClickHandler : IConsoleEntity
{
	bool CanFuncAdditionalClick();

	void OnFuncAdditionalClick();

	string GetFuncAdditionalClickHint();
}
