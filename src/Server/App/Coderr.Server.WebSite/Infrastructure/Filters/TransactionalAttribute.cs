﻿using Coderr.Server.Abstractions;
using Griffin.Data;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Coderr.Server.WebSite.Infrastructure.Filters
{
    public class TransactionalAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.HttpContext.Items["IgnoreTransaction"] as bool? == true)
                return;

            if (!HostConfig.Instance.IsConfigured)
            {
                return;
            }

            var isMethodTransactional = true;/*filterContext.HttpContext.Request.Method == "POST" ||
                                        filterContext.ActionDescriptor.FilterDescriptors.Any(x =>
                                            x.Filter.GetType() == typeof(TransactionalAttribute));*/
            if (filterContext.Exception == null && filterContext.ModelState.IsValid && isMethodTransactional)
            {
                var uow = (IAdoNetUnitOfWork) filterContext.HttpContext.RequestServices.GetService(typeof(IAdoNetUnitOfWork));

                //NULL when the setup is running.
                if (uow == null)
                    return;
                
                uow.SaveChanges();
            }
                
            base.OnActionExecuted(filterContext);
        }
    }
}
