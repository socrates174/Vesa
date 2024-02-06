using eShop.Ordering.OrderManagement.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure(builder.Configuration);

var app = builder.Build();
app.Configure();

app.Run();