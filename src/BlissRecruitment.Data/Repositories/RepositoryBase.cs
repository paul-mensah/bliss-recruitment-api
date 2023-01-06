using BlissRecruitment.Core.Domain;
using BlissRecruitment.Core.Repositories;
using BlissRecruitment.Data.Data;

namespace BlissRecruitment.Data.Repositories;

public class RepositoryBase<T> : IRepositoryBase<T> where T: EntityBase
{
    private readonly BlissRecruitmentDbContext _dbContext;

    protected RepositoryBase(BlissRecruitmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T> GetById(string id)
    {
        return await _dbContext.FindAsync<T>(id);
    }

    public async Task<bool> AddAsync(T entity)
    {
        await _dbContext.AddAsync(entity);
        int rows = await _dbContext.SaveChangesAsync();

        return rows > 0;
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        _dbContext.Update(entity);
        int rows = await _dbContext.SaveChangesAsync();

        return rows > 0;
    }
}