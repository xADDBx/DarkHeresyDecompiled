using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ConclusionPaginatorPoint : View<BlueprintConclusion>, IConclusionsUpdateHandler, ISubscriber
{
	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	[SerializeField]
	private GameObject m_RefutedParent;

	protected override void OnBind()
	{
		UpdateState();
	}

	private void UpdateState()
	{
		m_RefutedParent.Or(null)?.gameObject.SetActive(base.ViewModel.IsRefuted());
		if (Game.Instance.DetectiveSystem.HasItem(base.ViewModel))
		{
			m_StateSelectable.SetActiveLayer("Selected");
		}
		else if (Game.Instance.Player.UISettings.DetectiveSystemData.ExaminedDetectiveData.SelectedConclusions.IsEntityNew(base.ViewModel))
		{
			m_StateSelectable.SetActiveLayer("NewVariant");
		}
		else
		{
			m_StateSelectable.SetActiveLayer("Default");
		}
	}

	public void UpdateConclusions()
	{
		UpdateState();
	}
}
