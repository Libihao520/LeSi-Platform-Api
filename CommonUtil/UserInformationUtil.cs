using EFCoreMigrations;
using Microsoft.AspNetCore.Http;

namespace CommonUtil;

public class UserInformationUtil
{
    private readonly MyDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserInformationUtil(MyDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public long GetCurrentUserId()
    {
        var httpContextUser = _httpContextAccessor.HttpContext?.User;
        if (httpContextUser == null)
        {
            throw new InvalidOperationException("无法获取当前用户信息");
        }

        var claim = httpContextUser.Claims.FirstOrDefault(c => c.Type == "Id");
        if (claim == null || !long.TryParse(claim.Value, out var userId))
        {
            throw new InvalidOperationException("用户ID无效或不存在");
        }

        return userId;
    }

    public async Task<string> GetUserNameByIdAsync(long userId)
    {
        var findAsync = await _context.Users.FindAsync(userId);
        if (findAsync != null)
        {
            return findAsync.Name;
        }
        else
        {
            return "用户不存在！";
        }
    }

    public async Task<string> GetCurrentUserNameAsync()
    {
        var userId = GetCurrentUserId();
        return await GetUserNameByIdAsync(userId);
    }
}