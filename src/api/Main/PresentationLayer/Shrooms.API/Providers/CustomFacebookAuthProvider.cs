﻿using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using Autofac;
using Microsoft.Owin.Security.Facebook;
using Newtonsoft.Json.Linq;
using Shrooms.Domain.Services.Organizations;

namespace Shrooms.API.Providers
{
    public class CustomFacebookAuthProvider : FacebookAuthenticationProvider
    {
        public CustomFacebookAuthProvider(IContainer ioc)
        {
            OnAuthenticated = async context =>
            {
                foreach (var claim in context.User)
                {
                    if (claim.Key.Equals("first_name"))
                    {
                        string claimValue = claim.Value.ToString();
                        context.Identity.AddClaim(new System.Security.Claims.Claim(ClaimTypes.GivenName, claimValue));
                    }
                    else if (claim.Key.Equals("last_name"))
                    {
                        string claimValue = claim.Value.ToString();
                        context.Identity.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Surname, claimValue));
                    }
                    else if (claim.Key.Equals("picture"))
                    {
                        JObject json = JObject.Parse(claim.Value.ToString());
                        bool isDefaultImage;
                        bool.TryParse(json.SelectToken("data.is_silhouette").ToString(), out isDefaultImage);
                        if (isDefaultImage == false)
                        {
                            context.Identity.AddClaim(new System.Security.Claims.Claim("picture", json.SelectToken("data.url").ToString()));
                        }
                    }
                }
            };
            OnApplyRedirect = (FacebookApplyRedirectContext context) =>
            {
                using (var webReq = ioc.BeginLifetimeScope("AutofacWebRequest"))
                {
                    var org = webReq.Resolve(typeof(IOrganizationService)) as IOrganizationService;
                    var newRedirectUri = context.RedirectUri;
                    var organizationName = context.OwinContext.Get<string>("tenantName");
                    if (org.HasOrganizationEmailDomainRestriction(organizationName))
                    {
                        var validHostName = org.GetOrganizationHostName(organizationName);
                        var hostDomainParameter = CreateHostDomainParameter(validHostName);
                        newRedirectUri = $"{newRedirectUri}{hostDomainParameter}&organization={organizationName}";
                    }

                    context.Response.Redirect(newRedirectUri);
                }
            };
        }

        private static string CreateHostDomainParameter(string hostName) => $"&hd={hostName}";
    }
}