using Model.Dto.photo;
using Model.Dto.Yolo;
using Model.Other;

namespace Interface;

public interface IYoloService
{
    /// <summary>
    /// 获取表单
    /// </summary>
    /// <returns></returns>
    Task<ApiResult> getpkqTb(YoloDetectionQueryReq req);

    /// <summary>
    /// 上传照片识别
    /// </summary>
    /// <param name="po"></param>
    /// <returns></returns>
    Task<string> PutPhoto(PhotoAddDto po, CancellationToken cancellationToken);

    /// <summary>
    /// 获取表单
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<YoloPkqEditRes> GetPkqEdtTb(long id);

    /// <summary>
    /// 获取数据大屏数据
    /// </summary>
    /// <returns></returns>
    Task<YoloSjdpRes> Getsjdp();

    /// <summary>
    /// 根据id删除数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ApiResult> DeleteAsync(long id);

    /// <summary>
    /// 手动新增加数据
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    Task<ApiResult> AddDataTb(YoloDetectionPutReq req);
}