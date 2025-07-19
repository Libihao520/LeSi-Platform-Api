using Model.Dto.User;

namespace Interface;

public interface ICustomJwtService
{
    string GetToken(GetUserRes getUser);
}