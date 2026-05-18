using System;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Localization;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public sealed class TransitionMapEntityVM : ViewModel
{
	public readonly LocalizedString Name;

	public readonly MapEntityKind Kind;

	public readonly MapEntityState State;

	public readonly BlueprintMultiEntranceMap? Zone;

	private readonly Action m_Enter;

	private readonly Action m_Close;

	public TransitionMapEntityVM(LocalizedString name, MapEntityKind kind, MapEntityState state, Action enter, Action close, BlueprintMultiEntranceMap? zone = null)
	{
		Name = name;
		Kind = kind;
		State = state;
		m_Enter = enter;
		m_Close = close;
		Zone = zone;
	}

	public void OnClick()
	{
		m_Enter?.Invoke();
		m_Close?.Invoke();
	}
}
