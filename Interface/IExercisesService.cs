using Microsoft.AspNetCore.Mvc;
using Model.Dto.TestPaperManage;
using Model.Dto.TestPapers;
using Model.Entities;
using Model.Other;

namespace Interface;

public interface IExercisesService
{
    /// <summary>
    /// 获取题目
    /// </summary>
    /// <returns></returns>
    public Task<ApiResult> GetTestPapers(GetTestPapersReq req);

    /// <summary>
    /// 成绩提交
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    public Task<ApiResult> checkSubmit(SubMitExercisesReq req);

    /// <summary>
    /// 成绩中心获取列表
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    public Task<ApiResult> AchievementCenter(GetAchievementCenterReq req);

    /// <summary>
    /// 删除数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<ApiResult> DeleteService(long id);

    /// <summary>
    /// 下载wrod
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<byte[]> DownloadWord(long id);

    /// <summary>
    /// 题库管理
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    Task<ApiResult> GetTestPaperManage(GetTestPaperManageReq req);

    /// <summary>
    /// 批量导入题库
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    Task<ApiResult> AddTestPaperManage(AddTestPaperManageReq req);

    /// <summary>
    /// 获取科目或卷名
    /// </summary>
    /// <param name="subjectName"></param>
    /// <returns></returns>
    Task<ApiResult> GetSubjectsOrFileLabel(string? subjectName);

    /// <summary>
    /// 删除题库
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ApiResult> DeleteTestPaperManage(long id);

    /// <summary>
    /// 开启作答
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ApiResult> ChangeHasAnsweringStarted(long id);
}