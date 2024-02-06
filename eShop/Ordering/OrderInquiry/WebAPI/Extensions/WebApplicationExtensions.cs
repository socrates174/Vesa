using IoCloud.Shared.HttpRouting.Abstractions;

namespace eShop.Ordering.OrderInquiry.WebAPI.Extensions
{
    public static class WebApplicationExtensions
    {
        public static void Configure(this WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.Urls.Add("https://localhost:44315");
            }

            app.UseHttpsRedirection();

            var httpRouterCollection = app.Services.GetRequiredService<IHttpRouterCollection>();
            httpRouterCollection.Route(app);
        }
    }
}
