using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Text;


namespace Nop.Plugin.NopWarehouse.VendorReview.Extensions
{
    public static class TabStripHelper
    {
        /// <summary>
        /// Renders an additional tab to an element
        /// </summary>
        /// <param name="attachToElementWithId">The id of the element to attach to, e.g. product-edit</param>
        /// <param name="tabTitle">The title of the tab</param>
        /// <param name="contentUrl">The url to fetch the content of the tab</param>
        /// <param name="tabId">The id of the new tab. If null or empty, it is calculated from <paramref name="tabTitle"/></param>
        /// <returns></returns>
        public static HtmlString RenderAdminTab(string attachToElementWithId, string tabTitle, string contentUrl, string tabId = null)
        {
            tabId = string.IsNullOrEmpty(tabId)
                ? ("tab-" + tabTitle.Trim().ToLower().Replace(" ", "-").Replace("'", "\""))
                : tabId;
            return new HtmlString(
                "<script>"
                + "$(document).ready(function() {"
                + "$('#" + attachToElementWithId + " > .nav-tabs')"
                + ".append('<li><a data-tab-name=\"" + tabId + "\" data-toggle=\"tab\" href=\"#" + tabId + "\">" + tabTitle + "</a></li>');"
                + "$('#" + attachToElementWithId + " > .tab-content').append('<div class=\"tab-pane\" id=\"" + tabId + "\" data-contenturl=\"" + contentUrl + "\">Loading...</div>');});"
                + "$(document).one('click', 'a[data-tab-name=" + tabId + "]', function() {$(this).tab('show');"
                + "$.ajax({async: true,cache: false,type: 'GET',url: '" + contentUrl + "'})"
                + ".done(function(data) {$('#" + tabId + "').html(data);}); }); "
                + "</script>");
        }
    }
}
