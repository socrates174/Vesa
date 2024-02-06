using eShop.Ordering.OrderInquiry.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure(builder.Configuration);

var app = builder.Build();
app.Configure();

app.Run();
