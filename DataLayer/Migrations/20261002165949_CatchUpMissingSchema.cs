using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <summary>
    /// مایگریشن جبرانی: هم‌گام‌سازی دیتابیس با مدل فعلی.
    ///
    /// پیش از این، مایگریشن‌های پروژه ۱۷ جدول می‌ساختند در حالی که DbContext
    /// بیست‌وچهار DbSet دارد. هفت جدول و چندین ستون هرگز مایگریت نشده بودند و
    /// مسیر SQL Server (Program.cs → Database.Migrate) با خطا متوقف می‌شد.
    ///
    /// این مایگریشن تنها اشیای غایب را اضافه می‌کند: هر جدول با گارد
    /// OBJECT_ID و هر ستون با گارد COL_LENGTH بررسی می‌شود. بنابراین روی
    /// دیتابیسی که این اشیا را از قبل دارد (مثلاً با EnsureCreated ساخته شده)
    /// بدون خطا و بدون تغییر اجرا می‌شود، و روی دیتابیس تازه آن‌ها را می‌سازد.
    ///
    /// ستون‌های ALTER در یک دسته و هر CREATE TABLE در دسته‌ای جداگانه اجرا
    /// می‌شوند تا خطای احتمالی یک جدول، بقیه را متوقف نکند.
    ///
    /// ⚠️ این مایگریشن عمداً بازگشت‌پذیر نیست: اشیای موردنظر ممکن است پیش از
    /// این مایگریشن ساخته شده و حاوی داده واقعی باشند. حذف خودکار ریسک
    /// از دست رفتن داده دارد.
    ///
    /// نسخه SQL این مایگریشن برای بازبینی پیش از اجرا:
    /// DataLayer/Migrations/SQL/20261002165949_CatchUpMissingSchema.sql
    /// </summary>
    public partial class CatchUpMissingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'Tbl_Reservation', N'DiscountAmount') IS NULL
BEGIN
    ALTER TABLE [Tbl_Reservation] ADD [DiscountAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END

IF COL_LENGTH(N'Tbl_Reservation', N'DiscountCode') IS NULL
BEGIN
    ALTER TABLE [Tbl_Reservation] ADD [DiscountCode] nvarchar(50) NULL;
END

IF COL_LENGTH(N'Tbl_Reservation', N'PaidAmount') IS NULL
BEGIN
    ALTER TABLE [Tbl_Reservation] ADD [PaidAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END

IF COL_LENGTH(N'Tbl_Reservation', N'PayableAmount') IS NULL
BEGIN
    ALTER TABLE [Tbl_Reservation] ADD [PayableAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END

IF COL_LENGTH(N'Tbl_Reservation', N'PaymentMethod') IS NULL
BEGIN
    ALTER TABLE [Tbl_Reservation] ADD [PaymentMethod] int NOT NULL DEFAULT 0;
END

IF COL_LENGTH(N'Tbl_Reservation', N'PaymentStatus') IS NULL
BEGIN
    ALTER TABLE [Tbl_Reservation] ADD [PaymentStatus] int NOT NULL DEFAULT 0;
END

IF COL_LENGTH(N'Tbl_Reservation', N'RefundAmount') IS NULL
BEGIN
    ALTER TABLE [Tbl_Reservation] ADD [RefundAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END

IF COL_LENGTH(N'Tbl_Reservation', N'RefundBy') IS NULL
BEGIN
    ALTER TABLE [Tbl_Reservation] ADD [RefundBy] nvarchar(100) NULL;
END

IF COL_LENGTH(N'Tbl_Reservation', N'RefundDate') IS NULL
BEGIN
    ALTER TABLE [Tbl_Reservation] ADD [RefundDate] datetime2 NULL;
END

IF COL_LENGTH(N'Tbl_Reservation', N'RefundReference') IS NULL
BEGIN
    ALTER TABLE [Tbl_Reservation] ADD [RefundReference] nvarchar(100) NULL;
END

IF COL_LENGTH(N'Tbl_Reservation', N'RefundStatus') IS NULL
BEGIN
    ALTER TABLE [Tbl_Reservation] ADD [RefundStatus] nvarchar(50) NULL;
END

IF COL_LENGTH(N'Tbl_Reservation', N'RegisteredBy') IS NULL
BEGIN
    ALTER TABLE [Tbl_Reservation] ADD [RegisteredBy] nvarchar(100) NULL;
END

IF COL_LENGTH(N'Tbl_Reservation', N'ReservationType') IS NULL
BEGIN
    ALTER TABLE [Tbl_Reservation] ADD [ReservationType] int NOT NULL DEFAULT 0;
END

IF COL_LENGTH(N'Tbl_Reservation', N'TrackingCode') IS NULL
BEGIN
    ALTER TABLE [Tbl_Reservation] ADD [TrackingCode] nvarchar(50) NOT NULL DEFAULT N'';
END

IF COL_LENGTH(N'Tbl_Reservation', N'VariablePriceAgreed') IS NULL
BEGIN
    ALTER TABLE [Tbl_Reservation] ADD [VariablePriceAgreed] bit NOT NULL DEFAULT CAST(0 AS bit);
END

IF COL_LENGTH(N'Tbl_Personal', N'Avatar') IS NULL
BEGIN
    ALTER TABLE [Tbl_Personal] ADD [Avatar] nvarchar(250) NULL;
END

IF COL_LENGTH(N'Tbl_Personal', N'Bio') IS NULL
BEGIN
    ALTER TABLE [Tbl_Personal] ADD [Bio] nvarchar(1000) NULL;
END

IF COL_LENGTH(N'Tbl_Personal', N'ExperienceYears') IS NULL
BEGIN
    ALTER TABLE [Tbl_Personal] ADD [ExperienceYears] int NOT NULL DEFAULT 0;
END

IF COL_LENGTH(N'Tbl_Personal', N'FirstName') IS NULL
BEGIN
    ALTER TABLE [Tbl_Personal] ADD [FirstName] nvarchar(50) NOT NULL DEFAULT N'';
END

IF COL_LENGTH(N'Tbl_Personal', N'IsApproved') IS NULL
BEGIN
    ALTER TABLE [Tbl_Personal] ADD [IsApproved] bit NOT NULL DEFAULT CAST(0 AS bit);
END

IF COL_LENGTH(N'Tbl_Personal', N'LastName') IS NULL
BEGIN
    ALTER TABLE [Tbl_Personal] ADD [LastName] nvarchar(50) NOT NULL DEFAULT N'';
END

IF COL_LENGTH(N'Tbl_Personal', N'Line') IS NULL
BEGIN
    ALTER TABLE [Tbl_Personal] ADD [Line] nvarchar(100) NOT NULL DEFAULT N'';
END

IF COL_LENGTH(N'Tbl_Personal', N'Mobile') IS NULL
BEGIN
    ALTER TABLE [Tbl_Personal] ADD [Mobile] nvarchar(15) NOT NULL DEFAULT N'';
END

IF COL_LENGTH(N'Tbl_Personal', N'PersonalCode') IS NULL
BEGIN
    ALTER TABLE [Tbl_Personal] ADD [PersonalCode] nvarchar(50) NOT NULL DEFAULT N'';
END

IF COL_LENGTH(N'Tbl_Personal', N'Specialty') IS NULL
BEGIN
    ALTER TABLE [Tbl_Personal] ADD [Specialty] nvarchar(100) NOT NULL DEFAULT N'';
END

IF COL_LENGTH(N'Tbl_Personal', N'WorkingDays') IS NULL
BEGIN
    ALTER TABLE [Tbl_Personal] ADD [WorkingDays] nvarchar(100) NOT NULL DEFAULT N'';
END

IF COL_LENGTH(N'Tbl_Personal', N'WorkingHours') IS NULL
BEGIN
    ALTER TABLE [Tbl_Personal] ADD [WorkingHours] nvarchar(100) NOT NULL DEFAULT N'';
END

IF COL_LENGTH(N'Tbl_NewsDay', N'Author') IS NULL
BEGIN
    ALTER TABLE [Tbl_NewsDay] ADD [Author] nvarchar(100) NOT NULL DEFAULT N'';
END

IF COL_LENGTH(N'Tbl_NewsDay', N'Image') IS NULL
BEGIN
    ALTER TABLE [Tbl_NewsDay] ADD [Image] nvarchar(250) NULL;
END

IF COL_LENGTH(N'Tbl_NewsDay', N'IsPublished') IS NULL
BEGIN
    ALTER TABLE [Tbl_NewsDay] ADD [IsPublished] bit NOT NULL DEFAULT CAST(0 AS bit);
END

IF COL_LENGTH(N'Tbl_NewsDay', N'Title') IS NULL
BEGIN
    ALTER TABLE [Tbl_NewsDay] ADD [Title] nvarchar(200) NOT NULL DEFAULT N'';
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Tbl_Comment]', N'U') IS NULL
BEGIN
    CREATE TABLE [Tbl_Comment] (
        [ID] int NOT NULL IDENTITY,
        [CustomerName] nvarchar(100) NOT NULL,
        [ServiceName] nvarchar(100) NOT NULL,
        [Rating] int NOT NULL,
        [CommentText] nvarchar(1000) NOT NULL,
        [IsApproved] bit NOT NULL,
        [Tc] uniqueidentifier NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [RegisterData] datetime2 NOT NULL,
        CONSTRAINT [PK_Tbl_Comment] PRIMARY KEY ([ID])
    );
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Tbl_Discount]', N'U') IS NULL
BEGIN
    CREATE TABLE [Tbl_Discount] (
        [ID] int NOT NULL IDENTITY,
        [Code] nvarchar(50) NOT NULL,
        [Title] nvarchar(100) NOT NULL,
        [Percent] int NOT NULL,
        [MaxDiscount] decimal(18,2) NOT NULL,
        [MinPurchase] decimal(18,2) NOT NULL,
        [ExpireDate] datetime2 NULL,
        [UsageCount] int NOT NULL,
        [Tc] uniqueidentifier NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [RegisterData] datetime2 NOT NULL,
        CONSTRAINT [PK_Tbl_Discount] PRIMARY KEY ([ID])
    );
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Tbl_FAQ]', N'U') IS NULL
BEGIN
    CREATE TABLE [Tbl_FAQ] (
        [ID] int NOT NULL IDENTITY,
        [Category] nvarchar(100) NOT NULL,
        [Question] nvarchar(300) NOT NULL,
        [Answer] nvarchar(1500) NOT NULL,
        [OrderIndex] int NOT NULL,
        [IsPublished] bit NOT NULL,
        [Tc] uniqueidentifier NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [RegisterData] datetime2 NOT NULL,
        CONSTRAINT [PK_Tbl_FAQ] PRIMARY KEY ([ID])
    );
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Tbl_Notification]', N'U') IS NULL
BEGIN
    CREATE TABLE [Tbl_Notification] (
        [ID] int NOT NULL IDENTITY,
        [TargetRole] nvarchar(50) NOT NULL,
        [TargetUserTC] uniqueidentifier NULL,
        [Title] nvarchar(150) NOT NULL,
        [Message] nvarchar(500) NOT NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [Link] nvarchar(200) NULL,
        [Tc] uniqueidentifier NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [RegisterData] datetime2 NOT NULL,
        CONSTRAINT [PK_Tbl_Notification] PRIMARY KEY ([ID])
    );
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Tbl_PaymentTransaction]', N'U') IS NULL
BEGIN
    CREATE TABLE [Tbl_PaymentTransaction] (
        [ID] int NOT NULL IDENTITY,
        [ReservationTC] uniqueidentifier NOT NULL,
        [TrackingCode] nvarchar(50) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [GatewayName] nvarchar(50) NOT NULL,
        [Authority] nvarchar(100) NULL,
        [RefId] nvarchar(100) NULL,
        [Status] int NOT NULL,
        [StatusDescription] nvarchar(250) NULL,
        [IdempotencyKey] nvarchar(100) NOT NULL,
        [VerifiedAt] datetime2 NULL,
        [Tc] uniqueidentifier NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [RegisterData] datetime2 NOT NULL,
        CONSTRAINT [PK_Tbl_PaymentTransaction] PRIMARY KEY ([ID])
    );
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Tbl_SalonSetting]', N'U') IS NULL
BEGIN
    CREATE TABLE [Tbl_SalonSetting] (
        [ID] int NOT NULL IDENTITY,
        [SalonName] nvarchar(100) NOT NULL,
        [SalonLatinName] nvarchar(100) NOT NULL,
        [LogoUrl] nvarchar(300) NULL,
        [HeroSubtitle] nvarchar(150) NOT NULL,
        [IntroText] nvarchar(1000) NOT NULL,
        [AboutText] nvarchar(2000) NOT NULL,
        [IsSingleLineMode] bit NOT NULL,
        [ActiveLineSlug] nvarchar(50) NOT NULL,
        [OpeningHour] time NOT NULL,
        [ClosingHour] time NOT NULL,
        [OffHoursMessage] nvarchar(200) NOT NULL,
        [Theme] nvarchar(50) NOT NULL,
        [PhoneLandline] nvarchar(20) NOT NULL,
        [PhoneMobile] nvarchar(20) NOT NULL,
        [Email] nvarchar(100) NOT NULL,
        [Address] nvarchar(300) NOT NULL,
        [WorkingHoursDisplay] nvarchar(150) NOT NULL,
        [InstagramUrl] nvarchar(150) NOT NULL,
        [TelegramUrl] nvarchar(150) NOT NULL,
        [WhatsAppUrl] nvarchar(150) NOT NULL,
        [DomesticSocialUrl] nvarchar(150) NOT NULL,
        [HeroSliderImages] nvarchar(1000) NOT NULL,
        [Tc] uniqueidentifier NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [RegisterData] datetime2 NOT NULL,
        CONSTRAINT [PK_Tbl_SalonSetting] PRIMARY KEY ([ID])
    );
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Tbl_WaitingList]', N'U') IS NULL
BEGIN
    CREATE TABLE [Tbl_WaitingList] (
        [ID] int NOT NULL IDENTITY,
        [CustomerName] nvarchar(100) NOT NULL,
        [MobileNumber] nvarchar(15) NOT NULL,
        [ServiceName] nvarchar(150) NOT NULL,
        [PersonalName] nvarchar(100) NULL,
        [DesiredDate] datetime2 NOT NULL,
        [DesiredTime] time NOT NULL,
        [Status] int NOT NULL,
        [NotifyDeadline] datetime2 NULL,
        [Note] nvarchar(500) NULL,
        [Tc] uniqueidentifier NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [RegisterData] datetime2 NOT NULL,
        CONSTRAINT [PK_Tbl_WaitingList] PRIMARY KEY ([ID])
    );
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // عمداً خالی: این مایگریشن جبرانی بازگشت‌پذیر نیست.
            // اشیای ساخته‌شده ممکن است داده واقعی داشته باشند و پیش از این
            // مایگریشن هم وجود داشته باشند؛ حذف خودکار ریسک از دست رفتن
            // داده دارد و در صورت نیاز باید دستی و پس از پشتیبان‌گیری انجام شود.
            migrationBuilder.Sql("PRINT N'CatchUpMissingSchema بازگشت‌پذیر نیست — حذف دستی پس از پشتیبان‌گیری.';");
        }
    }
}
