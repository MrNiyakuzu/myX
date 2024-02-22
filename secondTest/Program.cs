using System.IO;
using System.Text.RegularExpressions;
using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

//var builder = WebApplication.CreateBuilder();

//var app = builder.Build();

//app.MapGet("/", (ApplicationContext db) => db.Users.ToList());

//app.Run();



// начальные данные
//List<User> users = new List<User>
//{
//    new() { Id = 1, Name = "Tom1", Age = 37 },
//    new() { Id = 2, Name = "Bob1", Age = 41 },
//    new() { Id = 3, Name = "Sam1", Age = 24 }
//};
//List<User> users = new List<User>
//{
//    new() { Id = Guid.NewGuid().ToString(), Name = "Tom1", Age = 37 },
//    new() { Id = Guid.NewGuid().ToString(), Name = "Bob1", Age = 41 },
//    new() { Id = Guid.NewGuid().ToString(), Name = "Sam1", Age = 24 }
//};

var builder = WebApplication.CreateBuilder();

string connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlite(connection));

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
//app.MapGet("/", (ApplicationContext db) => db.Users.ToList());

app.Map("/time", appBuilder =>
{
    var time = DateTime.Now.ToShortTimeString();

    // логгируем данные - выводим на консоль приложения
    appBuilder.Use(async (context, next) =>
    {
        Console.WriteLine($"Time: {time}");
        await next();   // вызываем следующий middleware
    });

    appBuilder.Run(async context => await context.Response.WriteAsync($"Time: {time}"));
}
);

app.Map("/image", appBuilder =>
{
    appBuilder.Run(async context =>
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync("wwwroot/image.html");
    }
    );
}
);

app.MapPost("/upload", async (context) =>
{
    Console.WriteLine("Зашел в загрузку картинок.");
    // путь к папке, где будут храниться файлы
    var uploadPath = $"{Directory.GetCurrentDirectory()}/uploads";
    // создаем папку для хранения файлов
    Directory.CreateDirectory(uploadPath);

    var request = context.Request;
    IFormFileCollection files = request.Form.Files;

    foreach (var file in files)
    {
        // путь к папке uploads
        string fullPath = $"{uploadPath}/{file.FileName}";

        // сохраняем файл в папку uploads
        using (var fileStream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
    }
    Console.WriteLine("Картинки загружены");
    await context.Response.WriteAsync("Файлы успешно загружены");
}
);

app.Map("/index.html", appBuilder =>
{
    appBuilder.Run(async context =>
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync("wwwroot/index.html");
    }
    );
}
);

//app.MapGet("/api/users", () => users);

//---------------------------------------------------------------------------------------------

//Обработка запросов с пользователями

app.Map("/users", appBuilder =>
{
    appBuilder.Run(async context =>
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync("wwwroot/users.html");
    }
    );
}
);

app.MapGet("/api/users", async (ApplicationContext db) =>
{
    Console.WriteLine("Получается список всех пользователей");
    var users = db.Users.ToList();
    Console.WriteLine("Список всех юзеров:");
    foreach (User u in users)
    {
        Console.WriteLine($"{u.Id} {u.Name}");
    }
    //await db.Users.ToListAsync();
    return Results.Json(users);
}
);

app.MapGet("/api/users/{id:int}", async (int id, ApplicationContext db) =>
{
    // получаем пользователя по id
    User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

    // если не найден, отправляем статусный код и сообщение об ошибке
    if (user == null) return Results.NotFound(new { message = "Пользователь не найден" });

    // если пользователь найден, отправляем его
    return Results.Json(user);
});

app.MapDelete("/api/users/{id:int}", async (int id, ApplicationContext db) =>
{
    // получаем пользователя по id
    User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

    // если не найден, отправляем статусный код и сообщение об ошибке
    if (user == null) return Results.NotFound(new { message = "Пользователь не найден" });

    // если пользователь найден, удаляем его
    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.Json(user);
});

app.MapPost("/api/users", async (User user, ApplicationContext db) =>
{
    // добавляем пользователя в массив
    await db.Users.AddAsync(user);
    await db.SaveChangesAsync();

    var users = db.Users.ToList();
    Console.WriteLine("Список новых юзеров:");
    foreach (User u in users)
    {
        Console.WriteLine($"{u.Id} {u.Name}");
    }
    return user;
});

app.MapPut("/api/users", async (User userData, ApplicationContext db) =>
{
    // получаем пользователя по id
    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userData.Id);

    // если не найден, отправляем статусный код и сообщение об ошибке
    if (user == null) return Results.NotFound(new { message = "Пользователь не найден" });

    // если пользователь найден, изменяем его данные и отправляем обратно клиенту
    user.Age = userData.Age;
    user.Name = userData.Name;
    await db.SaveChangesAsync();
    return Results.Json(user);
});

//---------------------------------------------------------------------------------------------

//Обработка запросов с постами

app.Map("/posts", appBuilder =>
{
    appBuilder.Run(async context =>
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync("wwwroot/posts.html");
    }
    );
}
);

app.MapGet("/api/posts", async (ApplicationContext db) =>
{
    Console.WriteLine("Получается список всех постов");
    var posts = db.Posts.ToList();
    Console.WriteLine("Список всех постов:");
    foreach (Post p in posts)
    {
        Console.WriteLine($"{p.Id} {p.Hashtag} {p.AuthorId}");
    }
    //await db.Users.ToListAsync();
    return Results.Json(posts);
}
);

app.MapGet("/api/posts/{id:int}", async (int id, ApplicationContext db) =>
{
    // получаем пост по id
    Post? post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id);

    // если не найден, отправляем статусный код и сообщение об ошибке
    if (post == null) return Results.NotFound(new { message = "Пост не найден" });

    // если найден, отправляем его
    return Results.Json(post);
});

app.MapDelete("/api/posts/{id:int}", async (int id, ApplicationContext db) =>
{
    // получаем пост по id
    Post? post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id);

    // если не найден, отправляем статусный код и сообщение об ошибке
    if (post == null) return Results.NotFound(new { message = "Пост не найден" });

    // если найден, удаляем его
    db.Posts.Remove(post);
    await db.SaveChangesAsync();
    return Results.Json(post);
});

app.MapPost("/api/posts", async (Post post, ApplicationContext db) =>
{
    if (post.Time == "")
    {
        post.Time = DateTime.Now.ToString();
    }
    // добавляем пост в массив
    await db.Posts.AddAsync(post);
    await db.SaveChangesAsync();

    var posts = db.Posts.ToList();
    Console.WriteLine("Список новых постов:");
    foreach (Post p in posts)
    {
        Console.WriteLine($"{p.Id} {p.Hashtag} {p.AuthorId}");
    }
    return post;
});

app.MapPut("/api/posts", async (Post postData, ApplicationContext db) =>
{
    // получаем пользователя по id
    var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == postData.Id);

    // если не найден, отправляем статусный код и сообщение об ошибке
    if (post == null) return Results.NotFound(new { message = "Пост не найден" });

    // если пользователь найден, изменяем его данные и отправляем обратно клиенту
    post.AuthorId = postData.AuthorId;
    post.Hashtag = postData.Hashtag;
    post.Time = postData.Time;
    post.Views = postData.Views;
    post.Text = postData.Text;
    await db.SaveChangesAsync();
    return Results.Json(post);
});

app.Run();