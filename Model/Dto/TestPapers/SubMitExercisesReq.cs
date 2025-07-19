namespace Model.Dto.TestPapers;

public class SubMitExercisesReq
{
    /// <summary>
    /// 试卷ID
    /// </summary>
    public long? TestPapersManageId { get; set; }
    public Dictionary<long,int> SingleChoice { get; set; }
    public Dictionary<long,List<int>> MultipleChoice { get; set; }
    public Dictionary<long,string> TrueFalseChoice { get; set; }
}