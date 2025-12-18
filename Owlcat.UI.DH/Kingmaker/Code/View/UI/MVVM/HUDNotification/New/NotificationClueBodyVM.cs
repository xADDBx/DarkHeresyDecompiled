using Kingmaker.Framework.DetectiveSystem;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class NotificationClueBodyVM : ViewModel
{
	public readonly BlueprintClue BlueprintClue;

	public readonly ObservableList<BlueprintClueAddendum> Addendums = new ObservableList<BlueprintClueAddendum>();

	private readonly ReactiveProperty<bool> m_IsNewClue = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> IsNewClue => m_IsNewClue;

	public NotificationClueBodyVM(BlueprintClue blueprintClue)
	{
		BlueprintClue = blueprintClue;
	}

	public void MarkAsNew()
	{
		m_IsNewClue.Value = true;
	}

	public void AddAddendum(BlueprintClueAddendum addendum)
	{
		if (!Addendums.Contains(addendum))
		{
			Addendums.Add(addendum);
		}
	}

	public void RemoveAddendum(BlueprintClueAddendum addendum)
	{
		if (Addendums.Contains(addendum))
		{
			Addendums.Remove(addendum);
		}
	}
}
