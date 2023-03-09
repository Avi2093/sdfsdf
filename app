"AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "Example.onmicrosoft.com",
    "TenantId": "",
    "ClientId": "",
    "CallbackPath": "/signin-oidc"
  },
  
  
  
   services.AddMicrosoftIdentityWebAppAuthentication(Configuration, "AzureAd");

            services.AddRazorPages().AddMvcOptions(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                              .RequireAuthenticatedUser()
                              .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });
