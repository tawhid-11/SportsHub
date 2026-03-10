using SportsHubBackend.BackgroundServices;
using SportsHubBackend.DBContext;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpClient<SportsHubBackend.Services.BKashService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
// Add services to the container.

builder.Services.AddControllers();
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

app.UseAuthorization();

app.MapControllers();

app.Run();
