using BlissRecruitment.Core.Domain;
using BlissRecruitment.Core.Models.Filters;

namespace BlissRecruitment.Core.Repositories;

public interface IQuestionsRepository : IRepositoryBase<QuestionEntity>
{
    Task<IEnumerable<QuestionEntity>> GetQuestionsByFilter(QuestionsFilter filter);
}