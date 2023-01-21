using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( c => 
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MembersInfoApi", Version = "v1" });
    c.AddSecurityDefinition("oauth2" , new OpenApiSecurityScheme 
    {
        Description = "Oauth2.0 which uses authorizationcode flow",
        Name = "oauth2.0",
        Type=SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(builder.Configuration["SwaggerAzureAD:AuthorizationUrl"]),
                TokenUrl = new Uri(builder.Configuration["SwaggerAzureAD:TokenUrl"]),
                Scopes = new Dictionary<string, string>
                {
                    { builder.Configuration["SwaggerAzureAD:Scope"],"Access API as user" }
                }
            }
        }
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement 
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference{Type=ReferenceType.SecurityScheme, Id="oauth2"}
            },
            new[] {builder.Configuration["SwaggerAzureAD:Scope"] }
        }
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApi(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.OAuthClientId(builder.Configuration["SwaggerAzureAD:ClientId"]);
        c.OAuthUsePkce();
        c.OAuthScopeSeparator(" ");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
