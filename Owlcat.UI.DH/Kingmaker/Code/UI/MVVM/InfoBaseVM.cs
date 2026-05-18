using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class InfoBaseVM : ViewModel
{
	private TooltipBaseTemplate m_MainTemplate;

	public readonly List<TooltipBrickVM> HeaderBricks = new List<TooltipBrickVM>();

	public readonly List<TooltipBrickVM> BodyBricks = new List<TooltipBrickVM>();

	public readonly List<TooltipBrickVM> FooterBricks = new List<TooltipBrickVM>();

	public readonly List<TooltipBrickVM> HintBricks = new List<TooltipBrickVM>();

	public TooltipBaseTemplate MainTemplate => m_MainTemplate;

	public IEnumerable<TooltipBaseTemplate> Templates { get; private set; }

	protected abstract TooltipTemplateType TemplateType { get; }

	public bool IsPrimitive
	{
		get
		{
			if (HeaderBricks.Count == 1 && BodyBricks.Empty())
			{
				return FooterBricks.Empty();
			}
			return false;
		}
	}

	public float ContentSpacing => m_MainTemplate.ContentSpacing;

	protected InfoBaseVM(TooltipData data)
	{
		if (data is CombinedTooltipData combinedTooltipData)
		{
			InitWithTemplates(combinedTooltipData.Templates);
		}
		else
		{
			NewTemplate(data.MainTemplate);
		}
	}

	protected InfoBaseVM(TooltipBaseTemplate template)
	{
		NewTemplate(template);
	}

	protected InfoBaseVM(IEnumerable<TooltipBaseTemplate> templates)
	{
		InitWithTemplates(templates);
	}

	public void SetNewTemplate(TooltipBaseTemplate template)
	{
		HeaderBricks.Clear();
		BodyBricks.Clear();
		FooterBricks.Clear();
		HintBricks.Clear();
		NewTemplate(template);
	}

	private void NewTemplate(TooltipBaseTemplate template)
	{
		m_MainTemplate = template;
		try
		{
			template.Prepare(TemplateType);
			CollectBricks(template.GetHeader(TemplateType), HeaderBricks);
			CollectBricks(template.GetBody(TemplateType), BodyBricks);
			CollectBricks(template.GetFooter(TemplateType), FooterBricks);
			CollectBricks(template.GetHint(TemplateType), HintBricks);
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create tooltip template: {arg}");
		}
	}

	private static void CollectBricks(IEnumerable<ITooltipBrick> bricks, List<TooltipBrickVM> bricksList)
	{
		bricksList.AddRange(bricks.OfType<TooltipBrickVM>());
	}

	private void InitWithTemplates(IEnumerable<TooltipBaseTemplate> templates)
	{
		Templates = templates;
		m_MainTemplate = templates.FirstOrDefault();
		foreach (TooltipBaseTemplate template in templates)
		{
			template.Prepare(TemplateType);
		}
		bool flag = true;
		foreach (TooltipBaseTemplate template2 in templates)
		{
			foreach (ITooltipBrick item2 in template2.GetHeader(TemplateType))
			{
				if (item2 is TooltipBrickVM item)
				{
					if (flag)
					{
						HeaderBricks.Add(item);
					}
					else
					{
						BodyBricks.Add(item);
					}
				}
			}
			CollectBricks(template2.GetBody(TemplateType), BodyBricks);
			flag = false;
		}
		CollectBricks(m_MainTemplate.GetFooter(TemplateType), FooterBricks);
		CollectBricks(m_MainTemplate.GetHint(TemplateType), HintBricks);
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		DisposeBricks(HeaderBricks);
		DisposeBricks(BodyBricks);
		DisposeBricks(FooterBricks);
		DisposeBricks(HintBricks);
	}

	private static void DisposeBricks(List<TooltipBrickVM> bricksList)
	{
		bricksList.ForEach(delegate(TooltipBrickVM b)
		{
			b.Dispose();
		});
		bricksList.Clear();
	}
}
