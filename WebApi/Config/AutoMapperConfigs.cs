using AutoMapper;
using CommonUtil;
using Model.Dto.AiModel;
using Model.Dto.Role;
using Model.Dto.TestPaperManage;
using Model.Dto.TestPapers;
using Model.Dto.User;
using Model.Dto.Yolo;
using Model.Entities;
using Model.Enum;

namespace WebApi.Config;

public class AutoMapperConfigs : Profile
{
    public AutoMapperConfigs()
    {
        //左往右
        //用户
        CreateMap<Users, GetUserRes>();
        //yolo
        CreateMap<YoLoTbs, YoloPkqRes>()
            .ForMember(dest => dest.CreateName, opt => opt.MapFrom(src => (src.CreateUserId)));

        CreateMap<YoloDetectionPutReq, YoLoTbs>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PhotosId, opt => opt.Ignore());

        CreateMap<YoLoTbs, YoloPkqEditRes>();

        //练题模块
        CreateMap<TestPapers, SingleChoice>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Topic))
            .ForMember(dest => dest.Options,
                opt => opt.MapFrom(src => new List<string>() { src.Choice1, src.Choice2, src.Choice3, src.Choice4 }));

        CreateMap<TestPapers, MultipleChoice>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Topic))
            .ForMember(dest => dest.Options,
                opt => opt.MapFrom(src => new List<string>() { src.Choice1, src.Choice2, src.Choice3, src.Choice4 }));

        CreateMap<TestPapers, TrueFalse>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Topic))
            .ForMember(dest => dest.Options,
                opt => opt.MapFrom(src => new List<string>() { src.Choice1, src.Choice2 }));

        // 题库模块
        CreateMap<TestPapersManage, GetTestPaperManageRes>()
            .ForMember(dest => dest.CreateName, opt => opt.MapFrom(src => (src.CreateUserId)));

        //成绩导出
        CreateMap<ReportCard, DownloadAchievementWordRes>();
        //只映射名称
        CreateMap<Users, DownloadAchievementWordRes>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

        CreateMap<ReportCard, AchievementCenterRes>();
        //RoleManagement
        CreateMap<Users, GetUserRoleRes>()
            .ForMember(dest => dest.Role, opt =>
                opt.MapFrom(src => EnumConvert.ConvertRoleNameToString(src.Role)))
            .AfterMap((src, dest) =>
            {
                dest.PassWord = AesUtilities.Decrypt(src.PassWord);
                dest.Email = AesUtilities.Decrypt(src.Email);
            });

        //RoleManagement
        CreateMap<AiModels, GetModelRes>()
            //desc目标对象--将源对象AiModels的CreateUserId属性的值作为目标对象GetModelRes的CreateName属性的值
            .ForMember(dest => dest.CreateName, opt => opt.MapFrom(src => (src.CreateUserId)));
    }
}