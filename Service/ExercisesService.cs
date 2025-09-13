using System.Reflection;
using System.Text.Json;
using AutoMapper;
using CommonUtil;
using CommonUtil.Extensions;
using CommonUtil.RandomIdUtil;
using CommonUtil.YoloUtil;
using EFCoreMigrations;
using Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Model;
using Model.Dto.TestPaperManage;
using Model.Dto.TestPapers;
using Model.Entities;
using Model.Other;
using OfficeOpenXml;
using SharpDocx;

namespace Service;

public class ExercisesService : IExercisesService

{
    private MyDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserInformationUtil _informationUtil;

    public ExercisesService(MyDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor,
        UserInformationUtil informationUtil)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _informationUtil = informationUtil;
    }

    public async Task<ApiResult> GetTestPapers(GetTestPapersReq req)
    {
        var mathematicsRes = new GetTestPapersRes()
        {
            SingleChoice = new List<SingleChoice>(),
            MultipleChoice = new List<MultipleChoice>(),
            TrueFalse = new List<TrueFalse>(),
        };

        var testPapersList = _context.TestPapers.Where(x => x.testPapersManageId == req.Id).ToList();
        foreach (var testPapers in testPapersList)
        {
            if (testPapers.type == (int)ExercisesType.单选题)
            {
                var map = _mapper.Map<SingleChoice>(testPapers);
                mathematicsRes.SingleChoice.Add(map);
            }
            else if (testPapers.type == (int)ExercisesType.多选题)
            {
                var map = _mapper.Map<MultipleChoice>(testPapers);
                mathematicsRes.MultipleChoice.Add(map);
            }
            else if (testPapers.type == (int)ExercisesType.判断题)
            {
                var map = _mapper.Map<TrueFalse>(testPapers);
                mathematicsRes.TrueFalse.Add(map);
            }
        }

        return ResultHelper.Success("获取成功！", mathematicsRes);
    }

    public async Task<ApiResult> checkSubmit(SubMitExercisesReq req)
    {
        var user = _httpContextAccessor.HttpContext.User;
        var createUserId = long.Parse(user.Claims.FirstOrDefault(c => c.Type == "Id").Value);

        // 检查是否已经提交过该试卷且未删除
        var existingReportCard = _context.ReportCards
            .FirstOrDefault(x => x.CreateUserId == createUserId
                                 && x.TestPapersManageId == req.TestPapersManageId
                                 && x.IsDeleted == 0);

        if (existingReportCard != null)
        {
            // 用户已提交过该试卷且未删除，不能重复提交
            return ResultHelper.Error("请勿重复提交！如需重新答题，请联系管理员删除之前的成绩记录！");
        }

        //得分
        int score = 0;
        int correctQuantity = 0;
        var testPapersList = _context.TestPapers.Where(x => x.testPapersManageId == req.TestPapersManageId).ToList();
        var singleChoices = testPapersList.Where(q => q.type == (int)ExercisesType.单选题)
            .Select(p => new { p.Id, p.TopicNumber, answer = p.answer[0], p.Grade })
            .OrderBy(s => s.TopicNumber)
            .ToList();

        foreach (var keyValuePair in req.SingleChoice)
        {
            var singleChoice = singleChoices.Where(q => q.Id == keyValuePair.Key).FirstOrDefault();
            if (singleChoice != null && singleChoice.answer == keyValuePair.Value)
            {
                score = score + singleChoice.Grade;
                correctQuantity++;
            }
        }

        // 处理多选题
        var multipleChoices = testPapersList.Where(m => m.type == (int)ExercisesType.多选题)
            .Select(m => new { m.Id, m.TopicNumber, CorrectAnswer = m.answer, m.Grade })
            .OrderBy(s => s.TopicNumber)
            .ToList();
        foreach (var keyValuePair in req.MultipleChoice)
        {
            var multipleChoice = multipleChoices.Where(q => q.Id == keyValuePair.Key).FirstOrDefault();
            if (multipleChoice.CorrectAnswer.SequenceEqual(keyValuePair.Value))
            {
                score = score + multipleChoice.Grade;
                correctQuantity++;
            }
        }

        //处理判断题
        var trueFalseChoices = testPapersList.Where(p => p.type == (int)ExercisesType.判断题)
            .Select(p => new { p.Id, p.TopicNumber, Answer = p.answer[0] == 1 ? "true" : "false", p.Grade })
            .OrderBy(p => p.TopicNumber)
            .ToList();

        foreach (var keyValuePair in req.TrueFalseChoice)
        {
            var trueFalseChoice = trueFalseChoices.Where(q => q.Id == keyValuePair.Key).FirstOrDefault();
            if (trueFalseChoice.Answer == keyValuePair.Value)
            {
                score = score + trueFalseChoice.Grade;
                correctQuantity++;
            }
        }


        var testPapersManage = _context.TestPapersManages.Where(x => x.Id == req.TestPapersManageId).FirstOrDefault();
        ReportCard reportCard1 = new ReportCard
        {
            Id = TimeBasedIdGeneratorUtil.GenerateId(),

            CreateDate = DateTime.Now,

            CreateUserId = createUserId,

            Subject = testPapersManage.FileLabel + "-" + testPapersManage.QuestionBankCourseTitle,
            //总分
            TotalPoints = score,
            //总数
            NumberOfQuestions = singleChoices.Count + multipleChoices.Count + trueFalseChoices.Count,
            // 答对数
            CorrectQuantity = correctQuantity,
            // 提交的答案
            SubmittedOptions = JsonSerializer.Serialize(req),

            TestPapersManageId = req.TestPapersManageId
        };
        _context.ReportCards.Add(reportCard1);
        _context.SaveChanges();
        return ResultHelper.Success("成功！", @$"本次答题得分为：{score} ,具体情况前往成绩中心查看！");
    }

    public async Task<ApiResult> AchievementCenter(GetAchievementCenterReq req)
    {
        var query = from reportCards in _context.ReportCards.Where(p => p.IsDeleted == 0)
                .OrderByDescending(q => q.CreateDate)
            join Users in _context.Users on reportCards.CreateUserId equals Users.Id into usersGroup
            from userItem in usersGroup.DefaultIfEmpty()
            select new AchievementCenterRes
            {
                Id = reportCards.Id,
                Subject = reportCards.Subject,
                TotalPoints = reportCards.TotalPoints,
                CreateDate = reportCards.CreateDate,
                NumberOfQuestions = reportCards.NumberOfQuestions,
                CorrectQuantity = reportCards.CorrectQuantity,
                CreateName = userItem == null ? null : userItem.Name,
            };

        var total = await query.CountAsync();
        var paginatedResult = await query
            .Skip((req.PageNum - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync();

        return ResultHelper.Success("查询成功", paginatedResult, total);
    }

    public async Task<ApiResult> DeleteService(long id)
    {
        try
        {
            var reportCard = await _context.ReportCards.FindAsync(id);
            // 不为空则执行软删除并且保存到数据库中
            if (reportCard != null)
            {
                reportCard.IsDeleted = 1;
                await _context.SaveChangesAsync();
            }

            return ResultHelper.Success("请求成功！", "数据已删除");
        }
        catch (Exception e)
        {
            return ResultHelper.Error("数据删除失败!");
        }
    }

    public async Task<byte[]> DownloadWord(long id)
    {
        var reportCard = await _context.ReportCards.FindAsync(id);
        SubMitExercisesReq subMitExercisesReq =
            JsonSerializer.Deserialize<SubMitExercisesReq>(reportCard.SubmittedOptions);
        var testPapersList = await _context.TestPapers.Where(q => q.testPapersManageId == reportCard.TestPapersManageId)
            .ToListAsync();
        var user = await _context.Users.FindAsync(reportCard.CreateUserId);


        var model = _mapper.Map<DownloadAchievementWordRes>(reportCard);
        model = _mapper.Map(user, model);

        foreach (var testPapers in testPapersList)
        {
            if (testPapers.type == (int)ExercisesType.单选题)
            {
                var map = _mapper.Map<SingleChoice>(testPapers);
                map.Answer = testPapers.answer[0].ToLetter();
                map.SubAnswer = subMitExercisesReq.SingleChoice[testPapers.Id].ToLetter();
                model.SingleChoice.Add(map);
            }
            else if (testPapers.type == (int)ExercisesType.多选题)
            {
                var map = _mapper.Map<MultipleChoice>(testPapers);
                map.Answer = string.Join(", ", testPapers.answer.Select(a => a.ToLetter().ToString()));
                map.SubAnswer = string.Join(", ",
                    subMitExercisesReq.MultipleChoice[testPapers.Id].Select(a => a.ToLetter().ToString()));
                model.MultipleChoice.Add(map);
            }
            else if (testPapers.type == (int)ExercisesType.判断题)
            {
                var map = _mapper.Map<TrueFalse>(testPapers);
                map.Answer = testPapers.answer[0] == 1 ? "正确" : "错误";
                map.SubAnswer = subMitExercisesReq.TrueFalseChoice[testPapers.Id] == "true" ? "正确" : "错误";
                model.TrueFalse.Add(map);
            }
        }


        var directoryName =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                "ExcelTemplate");
        var document = DocumentFactory.Create(Path.Combine(directoryName, "练题系统答题情况导出模板.docx"), model);
        var generateId = TimeBasedIdGeneratorUtil.GenerateId().ToString();
        document.Generate(Path.Combine(directoryName, @$"{generateId}结果.docx"));

        using var fileStream = new FileStream(Path.Combine(directoryName, @$"{generateId}结果.docx"), FileMode.Open,
            FileAccess.Read);
        using (var ms = new MemoryStream())
        {
            await fileStream.CopyToAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ms.ToArray();
        }
    }

    public async Task<ApiResult> GetTestPaperManage(GetTestPaperManageReq req)
    {
        try
        {
            var queryable = _context.TestPapersManages.Where(t => t.IsDeleted == 0).AsQueryable();
            if (!string.IsNullOrEmpty(req.FileLabel))
            {
                queryable = queryable.Where(t => t.FileLabel.Contains(req.FileLabel));
            }

            if (!string.IsNullOrEmpty(req.QuestionBankCourseTitle))
            {
                queryable = queryable.Where(t => t.QuestionBankCourseTitle.Contains(req.QuestionBankCourseTitle));
            }

            var total = await queryable.CountAsync();

            var listAsync = await queryable
                .Skip((req.PageNum - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

            var resultList = _mapper.Map<List<GetTestPaperManageRes>>(listAsync);

            foreach (var getTestPaperManageRes in resultList)
            {
                if (getTestPaperManageRes.CreateName != null)
                    getTestPaperManageRes.CreateName =
                        await _informationUtil.GetUserNameByIdAsync(long.Parse(getTestPaperManageRes.CreateName));
            }

            return ResultHelper.Success("查询成功", resultList, total);
        }

        catch (Exception e)
        {
            Console.WriteLine("$发生异常");
            return ResultHelper.Error("查询异常，请稍后重试");
        }
    }

    public async Task<ApiResult> AddTestPaperManage(AddTestPaperManageReq req)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //TODO 在TestPaperManage 新建一条数据 ，然后读取excel，把数据存到TestPaper
        try
        {
            string filePath = "";
            if (req.File != null)
            {
                //得：唯一文件名+扩展名
                string fileName = req.FileLabel + "_" + TimeBasedIdGeneratorUtil.GenerateId() + "_" +
                                  Path.GetExtension(req.File.FileName);
                //获取项目下现在的目录以及TempExcelFiles文件夹
                string saveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TempExcelFiles");
                Directory.CreateDirectory(saveDirectory);

                filePath = Path.Combine(saveDirectory, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await req.File.CopyToAsync(stream);
                }
            }

            var user = _httpContextAccessor?.HttpContext?.User;
            var createUserId = long.Parse(user.Claims.FirstOrDefault(c => c.Type == "Id").Value);
            var newRecord = new TestPapersManage()
            {
                Id = TimeBasedIdGeneratorUtil.GenerateId(),
                FileLabel = req.FileLabel,
                QuestionBankCourseTitle = req.QuestionBankCourseTitle,
                ExcelFilePath = filePath,
                CreateUserId = createUserId,
                CreateDate = DateTime.Now,
                IsDeleted = 0,
                HasAnsweringStarted = false
            };
            _context.TestPapersManages.Add(newRecord);

            //检查指定路径filePath的文件是否存在==>往下AI--注释占时不要删，还要看

            using (var stream = new MemoryStream())
            {
                await req.File.CopyToAsync(stream);
                stream.Position = 0; // 重置流的位置到起始点  

                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        ExercisesType testPapersType;
                        if (Enum.TryParse<ExercisesType>(worksheet.Cells[row, 1].Value.ToString(),
                                out ExercisesType result))
                        {
                            testPapersType = result;
                        }
                        else
                        {
                            return ResultHelper.Error($"导入失败，题目类型无法匹配！");
                        }

                        var paperData = new TestPapers()
                        {
                            Id = TimeBasedIdGeneratorUtil.GenerateId(),
                            TopicNumber = row - 1,
                            Subject = req.QuestionBankCourseTitle,
                            type = (int)testPapersType,
                            Topic = worksheet.Cells[row, 2].Value?.ToString(),
                            Choice1 = worksheet.Cells[row, 3].Value?.ToString(),
                            Choice2 = worksheet.Cells[row, 4].Value?.ToString(),
                            Choice3 = worksheet.Cells[row, 5].Value?.ToString(),
                            Choice4 = worksheet.Cells[row, 6].Value?.ToString(),
                            answer = new List<int>(),
                            Grade = int.Parse(worksheet.Cells[row, 8].Value?.ToString() ?? string.Empty),
                            testPapersManageId = newRecord.Id
                        };
                        paperData.answer =
                            ExcelDataParser.ParseAnswerFromCellValue(worksheet.Cells[row, 7].Value?.ToString());
                        _context.TestPapers.Add(paperData);
                    }
                }
            }

            await _context.SaveChangesAsync();

            return ResultHelper.Success("请求成功", "文件上传成功且数据保存完整");
        }
        catch (Exception e)
        {
            return ResultHelper.Error($"添加失败原因：{e.Message}");
        }
    }

    public async Task<ApiResult> GetSubjectsOrFileLabel(string? subjectName)
    {
        if (string.IsNullOrEmpty(subjectName))
        {
            var list = _context.TestPapersManages
                .Where(q => q.HasAnsweringStarted == true)
                .Where(q => q.IsDeleted == 0)
                .Select(q => q.QuestionBankCourseTitle).Distinct().ToList();
            var resultList = list.Select((title, index) => new Dictionary<string, object>
            {
                { "label", title },
                { "value", index + 1 }
            }).ToList();
            return ResultHelper.Success("请求成功！", resultList);
        }
        else
        {
            var testPapersManages =
                _context.TestPapersManages
                    .Where(q => q.QuestionBankCourseTitle == subjectName && q.HasAnsweringStarted == true)
                    .Select(q => new { q.Id, q.FileLabel })
                    .ToList();
            var resultList = testPapersManages.Select((item, index) => new Dictionary<string, object>
            {
                { "label", item.FileLabel },
                { "value", item.Id }
            }).ToList();
            return ResultHelper.Success("请求成功", resultList);
        }
    }

    public async Task<ApiResult> DeleteTestPaperManage(long id)
    {
        try
        {
            var testPapersManage = await _context.TestPapersManages.FindAsync(id);
            // 不为空则执行软删除并且保存到数据库中
            if (testPapersManage != null)
            {
                testPapersManage.IsDeleted = 1;
                await _context.SaveChangesAsync();
            }

            return ResultHelper.Success("请求成功！", "数据已删除");
        }
        catch (Exception e)
        {
            return ResultHelper.Error("数据删除失败!");
        }
    }

    public async Task<ApiResult> ChangeHasAnsweringStarted(long id)
    {
        var testPapersManage = await _context.TestPapersManages.FindAsync(id);
        if (testPapersManage != null)
        {
            testPapersManage.HasAnsweringStarted = !testPapersManage.HasAnsweringStarted;
            await _context.SaveChangesAsync();
            return ResultHelper.Success("请求成功！", "修改成功！");
        }
        else
        {
            return ResultHelper.Error("请刷新后重试！");
        }
    }
}