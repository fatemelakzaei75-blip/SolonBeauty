using DataCore.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DataCore.Services;

/// <summary>
/// ثبت متمرکز و استاندارد تمام سرویس‌های لایه DataCore در کانتینر Dependency Injection
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddDataCoreServices(this IServiceCollection services)
    {
        // سرویس‌های پایه سالن و لاین‌های خدمات
        services.AddScoped<ISalonService, SalonService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISalonServiceService, SalonServiceService>();
        services.AddScoped<ISocialService, SocialService>();
        services.AddScoped<INewsDayService, NewsDayService>();
        services.AddScoped<ISalonSettingService, SalonSettingService>();

        // سرویس‌های مشتریان و پرسنل
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IPersonalService, PersonalService>();
        services.AddScoped<IMobileService, MobileService>();
        services.AddScoped<IConfirmService, ConfirmService>();

        // سرویس‌های احراز هویت، نقش‌ها و دسترسی‌ها
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRolePermissionService, RolePermissionService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<UserRoleService>();

        // سرویس‌های رزرواسیون، مالی، تخفیف و لیست انتظار
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IDiscountService, DiscountService>();
        services.AddScoped<IWaitingListService, WaitingListService>();

        // سرویس‌های نمونه‌کار، لایک و بازدید
        services.AddScoped<ICategoryPortfolioService, CategoryPortfolioService>();
        services.AddScoped<IPortfolioService, PortfolioService>();
        services.AddScoped<ILikeService, LikeService>();
        services.AddSingleton<IPortfolioViewService, PortfolioViewService>();

        // سرویس‌های اعلانات، نظرات، سوالات متداول و گزارش‌ها
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IFAQService, FAQService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<ISeoService, SeoService>();

        return services;
    }
}
