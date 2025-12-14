using Azure.Identity;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using MunsonPickles.Web.Data;
using MunsonPickles.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Microsoft Entra External ID authentication
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

builder.Services.AddServerSideBlazor();

var sqlConnection = builder.Configuration["ConnectionStrings:Munson:SqlDb"];

builder.Services.AddSqlServer<PickleDbContext>(sqlConnection, options => options.EnableRetryOnFailure());
//builder.Services.addauthenticationstateserialization(OpenApiRouteHandlerBuilderExtensions =)

builder.Services.AddTransient<ProductService>();
builder.Services.AddTransient<ReviewService>();
var storageUrl = builder.Configuration["Storage:BlobServiceUrl"];

builder.Services.AddAzureClients(azureBuilder =>
{
    azureBuilder.AddBlobServiceClient(new Uri(storageUrl));
    azureBuilder.UseCredential(new DefaultAzureCredential());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.CreateDbIfNotExists();

app.Run();
