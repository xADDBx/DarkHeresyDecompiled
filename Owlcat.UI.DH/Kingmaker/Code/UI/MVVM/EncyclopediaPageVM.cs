using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Encyclopedia.Blocks;
using Kingmaker.ResourceLinks;
using Kingmaker.Settings;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaPageVM : ViewModel
{
	public readonly string Title;

	public readonly string GlossaryText;

	public readonly List<EncyclopediaPageBlockVM> BlockVMs = new List<EncyclopediaPageBlockVM>();

	private readonly List<EncyclopediaPageImageVM> m_ImageVMs = new List<EncyclopediaPageImageVM>();

	private readonly ReactiveProperty<EncyclopediaPageImageVM> m_FullscreenImageVM = new ReactiveProperty<EncyclopediaPageImageVM>();

	public readonly float FontMultiplier = FontSizeMultiplier;

	public IPage Page { get; }

	private static float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public EncyclopediaPageVM(IPage page)
	{
		BlueprintEncyclopediaGlossaryEntry blueprintEncyclopediaGlossaryEntry = null;
		if (page is BlueprintEncyclopediaGlossaryEntry)
		{
			blueprintEncyclopediaGlossaryEntry = (BlueprintEncyclopediaGlossaryEntry)page;
			page = ((BlueprintEncyclopediaGlossaryChapter)blueprintEncyclopediaGlossaryEntry.Parent).GetLetterIndexPage(page.GetTitle().Substring(0, 1));
		}
		Page = page;
		Title = page.GetTitle();
		if (page is BlueprintEncyclopediaPage { GlossaryEntry: not null } blueprintEncyclopediaPage)
		{
			string text = (blueprintEncyclopediaPage.GlossaryEntry as BlueprintEncyclopediaGlossaryEntry)?.GetDescription();
			GlossaryText = ((!string.IsNullOrWhiteSpace(text)) ? text : string.Empty);
		}
		foreach (IBlock block7 in page.GetBlocks())
		{
			EncyclopediaPageBlockVM encyclopediaPageBlockVM = null;
			try
			{
				if (!(block7 is BlueprintEncyclopediaBlockText block))
				{
					if (!(block7 is EncyclopediaEntryBlock block2))
					{
						if (!(block7 is BlueprintEncyclopediaBlockImage block3))
						{
							if (!(block7 is BlueprintEncyclopediaBlockBestiaryUnit block4))
							{
								if (!(block7 is BlueprintEncyclopediaBlockPages block5))
								{
									if (!(block7 is BlueprintEncyclopediaBookEventPage.BookEventLogBlock block6))
									{
										if (block7 is GlossaryEntryBlock glossaryEntryBlock)
										{
											bool marked = blueprintEncyclopediaGlossaryEntry == glossaryEntryBlock.Entry;
											encyclopediaPageBlockVM = new EncyclopediaPageBlockGlossaryEntryVM(glossaryEntryBlock, marked);
										}
									}
									else
									{
										encyclopediaPageBlockVM = new EncyclopediaPageBlockBookEventVM(block6);
									}
								}
								else
								{
									encyclopediaPageBlockVM = new EncyclopediaPageBlockChildPagesVM(block5);
								}
							}
							else
							{
								encyclopediaPageBlockVM = new EncyclopediaPageBlockUnitVM(block4);
							}
						}
						else
						{
							encyclopediaPageBlockVM = new EncyclopediaPageBlockImageVM(block3);
						}
					}
					else
					{
						encyclopediaPageBlockVM = new EncyclopediaPageBlockTextVM(block2);
					}
				}
				else
				{
					encyclopediaPageBlockVM = new EncyclopediaPageBlockTextVM(block);
				}
			}
			catch (Exception ex)
			{
				PFLog.UI.Exception(ex, "Can't create block: {0}", block7);
				continue;
			}
			if (encyclopediaPageBlockVM != null)
			{
				encyclopediaPageBlockVM.AddTo(this);
				BlockVMs.Add(encyclopediaPageBlockVM);
			}
		}
		Action<EncyclopediaPageImageVM> zoomAction = null;
		if (page is BlueprintEncyclopediaBookEventPage)
		{
			zoomAction = ShowFullscreenImage;
		}
		foreach (SpriteLink item2 in from spriteLink in page.GetImages()
			where spriteLink.Exists()
			select spriteLink)
		{
			EncyclopediaPageImageVM item = new EncyclopediaPageImageVM(item2.Load(), zoomAction).AddTo(this);
			m_ImageVMs.Add(item);
		}
	}

	protected override void OnDispose()
	{
		DisposeFullscreenImage();
	}

	private void ShowFullscreenImage(EncyclopediaPageImageVM imageVm)
	{
		if (m_FullscreenImageVM.Value == null)
		{
			m_FullscreenImageVM.Value = imageVm;
		}
	}

	private void DisposeFullscreenImage()
	{
		m_FullscreenImageVM.Value = null;
	}
}
