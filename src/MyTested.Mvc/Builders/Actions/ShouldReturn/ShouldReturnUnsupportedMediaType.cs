﻿namespace MyTested.Mvc.Builders.Actions.ShouldReturn
{
    using Contracts.Base;
    using Microsoft.AspNet.Mvc;

    /// <summary>
    /// Class containing methods for testing UnsupportedMediaTypeResult.
    /// </summary>
    /// <typeparam name="TActionResult">Result from invoked action in ASP.NET MVC 6 controller.</typeparam>
    public partial class ShouldReturnTestBuilder<TActionResult>
    {
        /// <summary>
        /// Tests whether action result is UnsupportedMediaTypeResult.
        /// </summary>
        /// <returns>Base test builder with action result.</returns>
        public IBaseTestBuilderWithActionResult<TActionResult> UnsupportedMediaType()
        {
            this.ValidateActionReturnType<UnsupportedMediaTypeResult>();
            return this.NewAndProvideTestBuilder();
        }
    }
}
