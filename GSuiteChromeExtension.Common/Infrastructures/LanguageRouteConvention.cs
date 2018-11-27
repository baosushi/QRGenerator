using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSuiteChromeExtension.Common.Infrastructures
{
    public class LanguageRouteConvention : IPageRouteModelConvention
    {

        public void Apply(PageRouteModel model)
        {
            var selectorCount = model.Selectors.Count;
            for (var i = 0; i < selectorCount; i++)
            {
                var selector = model.Selectors[i];
                model.Selectors.Add(new SelectorModel
                {
                    AttributeRouteModel = new AttributeRouteModel
                    {
                        Template = AttributeRouteModel.CombineTemplates("{language}", selector.AttributeRouteModel.Template),
                    }
                });
            }
        }

    }
}
