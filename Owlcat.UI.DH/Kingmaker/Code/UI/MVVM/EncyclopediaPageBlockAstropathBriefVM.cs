using Kingmaker.Blueprints.Encyclopedia;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaPageBlockAstropathBriefVM : EncyclopediaPageBlockVM
{
	private readonly ReactiveProperty<string> m_MessageLocation = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_MessageDate = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_MessageSender = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_MessageBody = new ReactiveProperty<string>();

	private readonly ReactiveProperty<bool> m_IsMessageRead = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<string> MessageLocation => m_MessageLocation;

	public ReadOnlyReactiveProperty<string> MessageDate => m_MessageDate;

	public ReadOnlyReactiveProperty<string> MessageSender => m_MessageSender;

	public ReadOnlyReactiveProperty<string> MessageBody => m_MessageBody;

	public ReadOnlyReactiveProperty<bool> IsMessageRead => m_IsMessageRead;

	public EncyclopediaPageBlockAstropathBriefVM(BlueprintEncyclopediaAstropathBriefPage.AstropathBriefBlock block)
		: base(block)
	{
		m_MessageLocation.Value = block.MessageLocation;
		m_MessageDate.Value = block.MessageDate;
		m_MessageSender.Value = block.MessageSender;
		m_MessageBody.Value = block.MessageBody;
		m_IsMessageRead.Value = block.IsMessageRead;
	}
}
