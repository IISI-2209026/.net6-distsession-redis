/*
 *  �ϥ� Redis �F������� Session
 */
using DistSession.Lib;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

#region Login Cookie
double LoginTimeout = builder.Configuration.GetValue<double>("LoginExpireMinute");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.AccessDeniedPath = new PathString("/Home/AccessDeny");  //�ڵ��M�����\�n�J�M�|����o�@��
        options.LoginPath = new PathString("/Login/Login");   //�n�J��
        options.LogoutPath = new PathString("/Home/Index");   //�n�X����쨺�@��
        options.ExpireTimeSpan = TimeSpan.FromMinutes(LoginTimeout);  //�p�G�S���o�@���]�w�M����w�]��14��(cookie�L�����ɶ�)
    });
#endregion

#region �N�ƾګO�@�����_�s�x��������
//�w�� Microsoft.AspNetCore.DataProtection.StackExchangeRedis
builder.Services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisConnection")), ".dist.Session");
#endregion

#region �[�J redis �@������
builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");  //redis �s�u
    options.InstanceName = ".dist.Session";
});
#endregion

#region use session
builder.Services.AddSession(options => {
    options.Cookie.Name = ".dist.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(300);     // �]�w Session �L�����ɶ�, 300 sec
});
#endregion

#region DI
builder.Services.AddScoped<Utils>();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();  //����
app.UseAuthorization();   //���v

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
