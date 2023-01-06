using BlissRecruitment.Core.Domain;

namespace BlissRecruitment.Core.Repositories;

public interface IRepositoryBase<T> where T : EntityBase
{
    Task<T> GetById(string id);
    Task<bool> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
}