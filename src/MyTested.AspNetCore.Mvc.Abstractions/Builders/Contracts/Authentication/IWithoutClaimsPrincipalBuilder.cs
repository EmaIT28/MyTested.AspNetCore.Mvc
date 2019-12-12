﻿namespace MyTested.AspNetCore.Mvc.Builders.Contracts.Authentication
{
    using System.Security.Claims;

    public interface IWithoutClaimsPrincipalBuilder
    {
        IAndWithoutClaimsPrincipalBuilder WithoutRole(string role);

        IAndWithoutClaimsPrincipalBuilder WithoutClaim(string type, string value);

        IAndWithoutClaimsPrincipalBuilder WithoutClaim(Claim claim);
    }
}
