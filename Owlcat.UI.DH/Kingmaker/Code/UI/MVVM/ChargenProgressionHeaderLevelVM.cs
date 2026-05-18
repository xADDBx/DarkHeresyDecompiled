using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ChargenProgressionHeaderLevelVM : ViewModel
{
	private readonly ReactiveProperty<bool> m_IsCompleted = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_Hovered = new ReactiveProperty<bool>(value: false);

	public readonly int Level;

	public ReadOnlyReactiveProperty<bool> IsCompleted => m_IsCompleted;

	public ReadOnlyReactiveProperty<bool> Hovered => m_Hovered;

	public ChargenProgressionHeaderLevelVM(int level, ReadOnlyReactiveProperty<int> maxFinishedLevel, ReadOnlyReactiveProperty<int> hoveredLevel)
	{
		Level = level;
		maxFinishedLevel.Subscribe(delegate(int l)
		{
			m_IsCompleted.Value = l >= Level;
		}).AddTo(this);
		hoveredLevel.Subscribe(delegate(int l)
		{
			m_Hovered.Value = l == Level;
		}).AddTo(this);
	}
}
