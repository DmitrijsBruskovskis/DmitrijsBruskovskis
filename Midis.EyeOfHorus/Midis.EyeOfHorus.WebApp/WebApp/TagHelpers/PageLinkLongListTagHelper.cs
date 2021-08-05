using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Midis.EyeOfHorus.WebApp.Models;

namespace Midis.EyeOfHorus.WebApp.TagHelpers
{
    public class PageLinkLongListTagHelper : TagHelper
    {
        private IUrlHelperFactory urlHelperFactory;
        public PageLinkLongListTagHelper(IUrlHelperFactory helperFactory)
        {
            urlHelperFactory = helperFactory;
        }
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }
        public PageViewModel PageModel { get; set; }
        public string PageAction { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);
            output.TagName = "div";

            // reference set will represent ul list
            TagBuilder tag = new TagBuilder("ul");
            tag.AddCssClass("pagination");

            // creating three references - to the current, previous and next page
            //TagBuilder currentItem = CreateTag(PageModel.PageNumber, urlHelper);

            // create a link to the previous page, if exist
            //if (PageModel.HasPreviousPage)
            //{
            //    TagBuilder prevItem = CreateTag(PageModel.PageNumber - 1, urlHelper);
            //    tag.InnerHtml.AppendHtml(prevItem);
            //}

            // create a link to the next page, if exist
            //if (PageModel.HasNextPage)
            //{
            //    TagBuilder nextItem = CreateTag(PageModel.PageNumber + 1, urlHelper);
            //    tag.InnerHtml.AppendHtml(nextItem);
            //}

            //create links to all pages
            for (int i = 1; i <= PageModel.TotalPages; i++)
            {
                TagBuilder currentItem = CreateTag(i, urlHelper);
                tag.InnerHtml.AppendHtml(currentItem);
            }

            output.Content.AppendHtml(tag);
        }

        TagBuilder CreateTag(int pageNumber, IUrlHelper urlHelper)
        {
            TagBuilder item = new TagBuilder("li");
            TagBuilder link = new TagBuilder("a");
            if (pageNumber == this.PageModel.PageNumber)
            {
                item.AddCssClass("active");
            }
            else
            {
                link.Attributes["href"] = urlHelper.Action(PageAction, new { page = pageNumber });
            }
            item.AddCssClass("page-item");
            link.AddCssClass("page-link");
            link.InnerHtml.Append(pageNumber.ToString());
            item.InnerHtml.AppendHtml(link);
            return item;
        }
    }
}
