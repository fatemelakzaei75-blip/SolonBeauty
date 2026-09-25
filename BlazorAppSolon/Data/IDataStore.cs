using DataLayer.Entity.BaseEntity;

namespace BlazorAppSolon.Data;

/// <summary>قرارداد دسترسی به داده برای پنل ها (پیاده سازی API یا نمونه محلی)</summary>
public interface IDataStore
{
    /// <summary>آیا داده از WebApi خوانده می شود؟</summary>
    bool IsOnline { get; }

    Task<List<T>> AllAsync<T>() where T : Tbl_BaseEntity;
    Task<T?> FindAsync<T>(Guid tc) where T : Tbl_BaseEntity;
    Task<bool> AddAsync<T>(T entity) where T : Tbl_BaseEntity;
    Task<bool> UpdateAsync<T>(T entity) where T : Tbl_BaseEntity;
    Task<bool> DeleteAsync<T>(Guid tc) where T : Tbl_BaseEntity;
    Task<int> CountAsync<T>() where T : Tbl_BaseEntity;
}
