using Microsoft.EntityFrameworkCore;
using newsApi.Data;
using newsApi.Services.AdminService;
using newsApi.Services.ImageService;
using newsApi.Services.StoryService;
using newsApi.Services.TagService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddScoped<IStoryService, StoryService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddCors(
    p =>
        p.AddPolicy("corspolicy",
    build =>
    {
        build.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
    }));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors("corspolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();