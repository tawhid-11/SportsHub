using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SportsHubBackend.BackgroundServices;
using SportsHubBackend.DBContext;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpClient<SportsHubBackend.Services.BKashService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                   .AllowCredentials()
                   .SetIsOriginAllowed(_ => true);
        });
});
builder.Services.AddHostedService<TournamentSchedulerService>();
builder.Services.AddScoped<SportsHubBackend.Services.IBKashService,
    SportsHubBackend.Services.BKashService>();
builder.Services.AddScoped<SportsHubBackend.Services.IEmailService,
    SportsHubBackend.Services.EmailService>();
builder.Services.AddSignalR();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAngular");
app.MapHub<SportsHubBackend.Hubs.SignalRHub>("/hubs");
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
