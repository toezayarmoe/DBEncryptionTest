using AspNetCoreRateLimit;
using DBEncryptionTest;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
//string dbUserId = "postgres";
//string dbPassword = "admin1234";

//var encryptedUserID = Encryption.EncryptString("b14ca5898a4e4133bbce2ea2315a1916", dbUserId);
//var encryptedPassword = Encryption.EncryptString("b14ca5898a4e4133bbce2ea2315a1916", dbPassword);

// Add services to the container.
var defaultConnection = builder.Configuration.GetConnectionString("NewConnection");
defaultConnection = Encryption.DecryptConnectionString(defaultConnection);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TestDBContext>(options =>
{
    options.UseNpgsql(defaultConnection);
});

builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = false;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.ClientIdHeader = "X-ClientId";
    options.GeneralRules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Endpoint = "*",
                Period = "10s",
                Limit = 2,
            }
        };
    // Whitelist specific endpoints (exclude these from rate-limiting)
    options.EndpointWhitelist = new List<string>
    {
        "Get:/api/WeatherForecast/*",
    };
});
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseIpRateLimiting();

app.UseAuthorization();

app.MapControllers();

app.Run();
