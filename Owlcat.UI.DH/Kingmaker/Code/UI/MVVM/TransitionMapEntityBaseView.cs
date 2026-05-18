using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Events;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class TransitionMapEntityBaseView : View<TransitionMapEntityVM>, ITransitionMapHighlight, ISubscriber
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_LocationName;

	[SerializeField]
	private OwlcatMultiSelectable m_KindSelectable;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	[SerializeField]
	private OwlcatMultiSelectable m_HighlightSelectable;

	[field: SerializeField]
	public OwlcatMultiButton LocationButton { get; private set; }

	protected override void OnBind()
	{
		base.OnBind();
		m_LocationName.text = base.ViewModel.Name.Text;
		m_KindSelectable.SetActiveLayer(base.ViewModel.Kind.ToString());
		m_StateSelectable.SetActiveLayer(base.ViewModel.State.ToString());
		base.gameObject.SetActive(value: true);
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		base.gameObject.SetActive(value: false);
	}

	public void SetKindLayer(string layer)
	{
		m_KindSelectable.SetActiveLayer(layer);
	}

	public void HandleHighlightEntry(string areaName, bool highlight)
	{
		if (!(base.ViewModel.Name != areaName))
		{
			m_HighlightSelectable.SetActiveLayer(highlight ? "Highlighted" : "Default");
		}
	}
}
