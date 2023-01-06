using BlissRecruitment.Core.Domain;
using BlissRecruitment.Core.Models.Filters;
using BlissRecruitment.Core.Repositories;
using BlissRecruitment.Data.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BlissRecruitment.Data.Repositories;

public sealed class QuestionsRepository : RepositoryBase<QuestionEntity>, IQuestionsRepository
{
    private readonly BlissRecruitmentDbContext _dbContext;

    public QuestionsRepository(BlissRecruitmentDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<QuestionEntity>> GetQuestionsByFilter(QuestionsFilter filter)
    {
        await Task.Delay(0);
        var questionsQueryable = _dbContext.Questions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(filter.Filter))
        {
            string choiceSearch = JsonConvert.SerializeObject(new List<QuestionChoice>
            {
                new QuestionChoice{ Choice = filter.Filter}
            });
            
            questionsQueryable = questionsQueryable.Where(x =>
                x.Question.ToLower().Contains(filter.Filter.ToLower()) || 
                EF.Functions.JsonContains(x.Choices,  choiceSearch));
        }
        
        return questionsQueryable
            .OrderByDescending(x => x.PublishedAt)
            .Skip(filter.Offset)
            .Take(filter.Limit);
    }
}