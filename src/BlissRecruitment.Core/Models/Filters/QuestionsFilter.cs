using System.ComponentModel.DataAnnotations;

namespace BlissRecruitment.Core.Models.Filters;

public class QuestionsFilter
{
    [Range(1, 100)] 
    public int Limit { get; set; } = 10;
    [Range(0, int.MaxValue)]
    public int Offset { get; set; } = 0;
    public string Filter { get; set; }
}

