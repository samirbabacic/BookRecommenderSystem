using BookRecommenderSystem.Repository;
using BookRecommenderSystem.Services;
using BookRecommenderSystem.Settings;
using Microsoft.Extensions.Options;

namespace BookRecommenderSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            AddDependencyInjection(builder.Services, builder.Configuration);

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        public static IServiceCollection AddDependencyInjection(IServiceCollection services, IConfiguration configuration)
        {
            //Database config
            services.Configure<DbSettings>(configuration.GetSection("DbConnectionSettings"));
            
            //Repository
            services.AddSingleton(typeof(IRepository<>), typeof(Repository<>));

            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IBookService, BookService>();

            return services;
        }
    }
}