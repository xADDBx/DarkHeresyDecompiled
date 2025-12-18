using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class JournalDetectiveQuestPCView : JournalQuestPCView
{
	[Header("Detective Elements")]
	[SerializeField]
	private Image m_DetectiveQuestIcon;

	[SerializeField]
	private Sprite m_DefaultIcon;

	[SerializeField]
	private OwlcatMultiButton m_ToCaseButton;

	[SerializeField]
	private TMP_Text m_ToCaseLabel;

	protected override void OnBind()
	{
		base.OnBind();
		m_DetectiveQuestIcon.sprite = base.ViewModel.RelatedCase?.Icon ?? m_DefaultIcon;
		ObservableSubscribeExtensions.Subscribe(m_ToCaseButton.OnLeftClickAsObservable(), delegate
		{
			EventBus.RaiseEvent(delegate(IDetectiveJournalUIHandler h)
			{
				h.HandleOpenDetectiveJournal(base.ViewModel.RelatedCase);
			});
		}).AddTo(this);
		m_ToCaseLabel.text = UIStrings.Instance.QuesJournalTexts.OpenCase.Text;
		GameObject obj = m_ToCaseButton.gameObject;
		BlueprintCase relatedCase = base.ViewModel.RelatedCase;
		obj.SetActive(relatedCase != null && !relatedCase.IsUnknown());
	}
}
