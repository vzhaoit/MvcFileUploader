using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace MvcFileUploader.Models
{
    using System.Web.Mvc;

    public class MvcFileUploadModelBuilder : IMvcFileUploadModelBuilder
    {
        private readonly HtmlHelper _helper;

        private string _returnUrl = "#";
        private string _fileTypes = ""; // jquery-image-upload format expression string
        private long _maxFileSizeInBytes = 10485760; // in bytes, default 10MB
        private string _uploadUrl = "/upload"; 
        private string _renderType = "link";
        private string _template = "_MvcFileUpload";

        private bool _includeScriptAndTemplate = true;


        private UploadUI _uiStyle = UploadUI.Bootstrap;
        private string _popupLabelTxt="Upload"; 

        //post additional values        
        private readonly IDictionary<string, string> _postValuesWithUpload=new Dictionary<string, string>();


        private FileUploadViewModel GetViewModel()
        {
            return new FileUploadViewModel()
                       {
                           FileTypes = _fileTypes,
                           MaxFileSizeInBytes = _maxFileSizeInBytes,
                           UploadUrl = _uploadUrl,
                           UIStyle = _uiStyle,
                           ReturnUrl = _returnUrl??"#",
                           RenderSharedScript = _includeScriptAndTemplate,
                           PostValuesWithUpload = _postValuesWithUpload,
                           ShowPopUpClose = "link".Equals(_renderType) && _returnUrl != null
                       };
        }
        
        public MvcFileUploadModelBuilder(HtmlHelper helper)
        {
            _helper = helper;            
        }


        public IMvcFileUploadModelBuilder UploadAt(string url)
        {
            _uploadUrl = url;

            return this;
        }

        public IMvcFileUploadModelBuilder ReturnAt(string url)
        {
            _returnUrl = url;

            return this;
        }

        public IMvcFileUploadModelBuilder WithFileTypes(string fileTypes)
        {
            _fileTypes = fileTypes;

            return this;
        }

        public IMvcFileUploadModelBuilder WithMaxFileSize(long sizeInBytes)
        {
            _maxFileSizeInBytes = sizeInBytes;

            return this;
        }

        public IMvcFileUploadModelBuilder AddFormField(string fieldName, string fieldValue)
        {
            _postValuesWithUpload.Add(fieldName, fieldValue);
            return this;
        }


        /// <summary>
        /// Excludes the shared script. 
        /// Should be called when rendering two inline upload widget
        /// </summary>
        /// <returns></returns>
        public IMvcFileUploadModelBuilder ExcludeSharedScript()
        {
            _includeScriptAndTemplate = false;

            return this;
        }

        public IHtmlString RenderInline()
        {
            _renderType = "inline";
            return _helper.Partial(_template, GetViewModel());
        }

        public IHtmlString RenderInline(string template)
        {
            _template = template;
            return RenderInline();
        }

        public IHtmlString RenderPopup(string labelTxt, string dataTarget, object htmlAttributes = null)
        {
            _includeScriptAndTemplate = true;
            _renderType = "link";
            _popupLabelTxt = labelTxt;

            
            var tag = new TagBuilder("a");
            var urlHelper = new UrlHelper(_helper.ViewContext.RequestContext);
                        
            var linkUrl = urlHelper.Action("UploadDialog", "MvcFileUpload", GetViewModel());
            int i = 0;
            foreach (var postVal in _postValuesWithUpload)
            {
                linkUrl += String.Format("&postValues[{0}].Key={1}", i, HttpUtility.UrlEncode(postVal.Key));
                linkUrl += String.Format("&postValues[{0}].Value={1}", i, HttpUtility.UrlEncode(postVal.Value));
                i++;
            } 


            tag.Attributes.Add("href", linkUrl);

            tag.Attributes.Add("role", "button");
            tag.Attributes.Add("data-toggle", "modal");
            tag.Attributes.Add("data-target", dataTarget);


            tag.InnerHtml = _popupLabelTxt;

            if (htmlAttributes != null)
                htmlAttributes.GetType().GetProperties().ToList().ForEach(p => tag.Attributes.Add(p.Name, p.GetValue(htmlAttributes, null).ToString()));

            return new MvcHtmlString(tag.ToString());
        }
    }
}