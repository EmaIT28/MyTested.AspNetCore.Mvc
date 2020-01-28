﻿namespace MyTested.AspNetCore.Mvc.Internal.Routing
{
    using System;
    using System.Linq;
    using Contracts;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Utilities;
    using Utilities.Extensions;

    /// <summary>
    /// Used for resolving HTTP request to a route.
    /// </summary>
    public static class MvcRouteResolver
    {
        /// <summary>
        /// Resolves HTTP request to a route using the provided route context and the action selector and invoker services.
        /// </summary>
        /// <param name="services">Application services from which the route will be resolved.</param>
        /// <param name="router">IRouter to resolve route values.</param>
        /// <param name="routeContext">RouteContext to use for resolving the route values.</param>
        /// <returns>Resolved route information.</returns>
        public static ResolvedRouteContext Resolve(IServiceProvider services, IRouter router, RouteContext routeContext)
        {
            try
            {
                RouteDataResolver.ResolveRouteData(router, routeContext);
            }
            catch (Exception ex)
            {
                return new ResolvedRouteContext($"exception was thrown when trying to resolve route data: '{ex.Unwrap().Message}'");
            }

            var actionSelector = services.GetRequiredService<IActionSelector>();

            ActionDescriptor actionDescriptor;
            try
            {
                var actions = routeContext
                    .RouteData
                    .Routers
                    .OfType<MvcAttributeRouteHandler>()
                    .FirstOrDefault()
                    ?.Actions
                    ?? actionSelector.SelectCandidates(routeContext);
                
                actionDescriptor = actionSelector.SelectBestCandidate(
                    routeContext,
                    actions);
            }
            catch (Exception ex)
            {
                return new ResolvedRouteContext($"exception was thrown when trying to select an action: '{ex.Unwrap().Message}'");
            }

            if (actionDescriptor == null)
            {
                return new ResolvedRouteContext("action could not be matched");
            }

            var actionContext = new ActionContext(routeContext.HttpContext, routeContext.RouteData, actionDescriptor);

            if (!(actionDescriptor is ControllerActionDescriptor controllerActionDescriptor))
            {
                throw new InvalidOperationException("Only controller actions are supported by the route testing.");
            }

            var actionInvokerFactory = services.GetRequiredService<IActionInvokerFactory>();

            var invoker = actionInvokerFactory.CreateInvoker(actionContext);
            if (!(invoker is IModelBindingActionInvoker modelBindingActionInvoker))
            {
                throw new InvalidOperationException($"Route testing requires the selected {nameof(IActionInvoker)} by the {nameof(IActionInvokerFactory)} to implement {nameof(IModelBindingActionInvoker)}.");
            }

            try
            {
                AsyncHelper.RunSync(() => modelBindingActionInvoker.InvokeAsync());
            }
            catch (Exception ex)
            {
                return new ResolvedRouteContext($"exception was thrown when trying to bind the action arguments: '{ex.Unwrap().Message}'");
            }

            if (modelBindingActionInvoker.BoundActionArguments == null)
            {
                return new ResolvedRouteContext("action could not be invoked because of the declared filters. You must set the request properties so that they will pass through the pipeline");
            }

            return new ResolvedRouteContext(
                controllerActionDescriptor.ControllerTypeInfo,
                controllerActionDescriptor.ControllerName,
                controllerActionDescriptor.ActionName,
                modelBindingActionInvoker.BoundActionArguments,
                actionContext.RouteData,
                actionContext.ModelState);
        }
    }
}
