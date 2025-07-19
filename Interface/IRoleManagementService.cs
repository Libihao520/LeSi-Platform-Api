using Model.Dto.Role;
using Model.Other;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Interface;

public interface IRoleManagementService
{
    Task<ApiResult> GetUserRole(GetUserRoleReq req);

    Task<ApiResult> DeleteUserRoleAsync(long id);

    Task<ApiResult> AddOrUpdateUserRole(PutUserRoleRes res);

    Task<ApiResult> ImportUsersFromExcel(IFormFile file);

    Task<byte[]> DownloadExcelUsersFromExcel();
}