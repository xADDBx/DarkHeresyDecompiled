using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BookEventChooseCharacterPCView : View<BookEventChooseCharacterVM>
{
	[SerializeField]
	private FadeAnimator m_WindowAnimator;

	[SerializeField]
	private Button m_CloseButton;

	[Header("Characters Block")]
	[SerializeField]
	private WidgetList m_WidgetListCharacter;

	[SerializeField]
	private BookEventCharacterPCView m_BookEventCharacterViewPrefab;

	[Header("Skills Block")]
	[SerializeField]
	private WidgetList m_WidgetListSkills;

	[SerializeField]
	private BookEventSkillsBlockPCView m_BookEventSkillsBlockViewPrefab;

	[Header("Confirm")]
	[SerializeField]
	private Button m_ConfirmButton;

	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_ConfirmLabel;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_WindowAnimator.Initialize();
			m_Title.text = UIStrings.Instance.SkillcheckTooltips.ChooseCharacter;
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		Show();
		base.ViewModel.SelectedIndex.Subscribe(OnChoose).AddTo(this);
		m_ConfirmButton.interactable = false;
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnClickAsObservable(), delegate
		{
			Hide();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ConfirmButton.OnClickAsObservable(), delegate
		{
			Confirm();
		}).AddTo(this);
		DrawCharacterEntities();
		DrawSkillsEntities();
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	private void DrawCharacterEntities()
	{
		m_WidgetListCharacter.DrawEntries(base.ViewModel.BookEventCharacters.ToArray(), m_BookEventCharacterViewPrefab);
	}

	private void DrawSkillsEntities()
	{
		m_WidgetListSkills.DrawEntries(base.ViewModel.BookEventSkillsBlocks.ToArray(), m_BookEventSkillsBlockViewPrefab);
	}

	private void Confirm()
	{
		base.ViewModel.ConfirmUnit();
		Hide();
	}

	private void OnChoose(int? index)
	{
		m_WidgetListSkills.Entries?.ForEach(delegate(IBindable s)
		{
			(s as BookEventSkillsBlockPCView)?.SelectSkill(index);
		});
		m_ConfirmButton.interactable = index.HasValue;
	}

	private void Show()
	{
		m_WindowAnimator.AppearAnimation();
	}

	private void Hide()
	{
		EventBus.RaiseEvent(delegate(IBookEventUIHandler h)
		{
			h.HandleChooseCharacterEnd();
		});
		m_WindowAnimator.DisappearAnimation();
	}
}
