using System.Net;
using AutoMapper;
using CommonUtil;
using CommonUtil.RandomIdUtil;
using EFCoreMigrations;
using Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Model;
using Model.Dto.Role;
using Model.Dto.User;
using Model.Entities;
using Model.Enum;
using Model.Other;
using MySqlConnector;
using OfficeOpenXml;

namespace Service;

public class RoleManagementService : IRoleManagementService
{
    private readonly ILogger<RoleManagementService> _logger;
    private readonly MyDbContext _context;
    private readonly IMapper _mapper;
    private readonly UserInformationUtil _informationUtil;

    public RoleManagementService(MyDbContext context, IMapper mapper,
        ILogger<RoleManagementService> logger, UserInformationUtil informationUtil)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _informationUtil = informationUtil;
    }

    public async Task<ApiResult> GetUserRole(GetUserRoleReq req)
    {
        if (req.Id == null)
        {
            var userstb = _context.Users.Where(q => q.IsDeleted == 0);
            if (!string.IsNullOrEmpty(req.username))
            {
                userstb = userstb.Where(q => q.Name.Contains(req.username));
            }

            var total = await userstb.CountAsync();

            var paginatedResult = await userstb
                .Skip((req.PageNum - 1) * req.PageSize) // 跳过前面的记录  
                .Take(req.PageSize) // 取接下来的指定数量的记录  
                .ToListAsync(); // 转换为列表  

            var resList = _mapper.Map<List<GetUserRoleRes>>(paginatedResult);
            return ResultHelper.Success("查询成功", resList, total);
        }
        else
        {
            var usersEnumerable = _context.Users.Where(u => u.Id == req.Id).FirstOrDefault();
            if (usersEnumerable == null)
            {
                return ResultHelper.Error("获取用户信息失败！");
            }

            var res = _mapper.Map<GetUserRoleRes>(usersEnumerable);
            return ResultHelper.Success("查询成功", res);
        }
    }

    public async Task<ApiResult> DeleteUserRoleAsync(long id)
    {
        try
        {
            // 根据id查找对象
            var findAsync = await _context.Users.FindAsync(id);
            // 不为空则执行软删除并且保存到数据库中
            if (findAsync != null)
            {
                findAsync.IsDeleted = 1;
                await _context.SaveChangesAsync();
            }

            return ResultHelper.Success("请求成功！", "数据已删除");
        }
        catch (Exception e)
        {
            return ResultHelper.Error("用户数据删除失败");
        }
    }

    public async Task<ApiResult> AddOrUpdateUserRole(PutUserRoleRes res)
    {
        try
        {
            //有id是编辑，无id是新增用户
            if (res.Id == null)
            {
                var user = _context.Users.Where(q => q.Name == res.Name).FirstOrDefault();
                if (user != null)
                {
                    return ResultHelper.Error("用户名称已被注册了，请换一个！");
                }

                var createUserId = _informationUtil.GetCurrentUserId();
                //TODO 验证邮箱是否合规（最好是将注册的验证邮箱抽出来通用） CreateUserId采用当前操作角色的id 
                Users insterUser = new Users()
                {
                    Id = TimeBasedIdGeneratorUtil.GenerateId(),
                    Name = res.Name,
                    PassWord = res.PassWord,
                    Email = res.Email,
                    CreateDate = DateTime.Now,
                    CreateUserId = createUserId,
                    Role = res.Role,
                    IsDeleted = 0
                };
                _context.Users.Add(insterUser);
                _context.SaveChanges();
                return ResultHelper.Success("请求成功！", "新增用户成功！");
            }
            else
            {
                var user = _context.Users.Where(q => q.Id == res.Id).FirstOrDefault();
                if (user == null)
                {
                    return ResultHelper.Error("用户不存在 ");
                }
                else
                {
                    user.PassWord = res.PassWord;
                    user.Email = res.Email;
                    user.Role = res.Role;
                    await _context.SaveChangesAsync();
                    return ResultHelper.Success("请求成功！", "密码更新完成");
                }
            }
        }
        catch (DbUpdateConcurrencyException concurrencyException)
        {
            _logger.LogError(concurrencyException, "用户数据操作更新时发生并发冲突");
            return ResultHelper.Error("数据已被修改，请刷新后重试。");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用户数据更新时发生错误");
            return ResultHelper.Error("未知错误，请刷新后重试！");
        }
    }

    public async Task<ApiResult> ImportUsersFromExcel(IFormFile file)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //异步事务
        using var transaction = await _context.Database.BeginTransactionAsync();

        var createUserId = _informationUtil.GetCurrentUserId();
        var usersToAdd = new List<Users>();
        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            stream.Position = 0; // 重置流的位置到起始点  

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    var userName = worksheet.Cells[row, 1].Text;
                    AuthorizeRoleName role;
                    role = EnumConvert.ConvertStringToRoleName(worksheet.Cells[row, 2].Text);
                    var userEmail = AesUtilities.Encrypt(worksheet.Cells[row, 3].Text);
                    var userPassword = AesUtilities.Encrypt(worksheet.Cells[row, 4].Text);

                    usersToAdd.Add(
                        new Users()
                        {
                            Id = TimeBasedIdGeneratorUtil.GenerateId(),
                            Name = userName,
                            PassWord = userPassword,
                            Email = userEmail,
                            CreateDate = DateTime.Now,
                            CreateUserId = createUserId,
                            Role = role,
                            IsDeleted = 0
                        }
                    );
                }
            }
        }

        try
        {
            await _context.Users.AddRangeAsync(usersToAdd);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 })
        {
            await transaction.RollbackAsync();
            _logger.LogError("导入数据包含重复记录");
            return ResultHelper.Error("存在重复用户名，请检查Excel数据");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "用户导入失败");
            return ResultHelper.Error("导入失败！");
        }

        return ResultHelper.Success("请求成功！", "导入用户成功！");
    }

    public async Task<byte[]> DownloadExcelUsersFromExcel()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var users = await _context.Users.Where(q => q.IsDeleted == 0).ToListAsync();

        using (var ms = new MemoryStream())
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Users");

                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "用户名";
                worksheet.Cells[1, 3].Value = "邮箱";
                worksheet.Cells[1, 4].Value = "创建时间";

                int row = 2;
                foreach (var user in users)
                {
                    worksheet.Cells[row, 1].Value = user.Id;
                    worksheet.Cells[row, 2].Value = user.Name;
                    worksheet.Cells[row, 3].Value = AesUtilities.Decrypt(user.Email);
                    worksheet.Cells[row, 4].Value = user.CreateDate;

                    row++;
                }

                package.SaveAs(ms);
            }

            ms.Seek(0, SeekOrigin.Begin);
            return ms.ToArray();
        }
    }
}