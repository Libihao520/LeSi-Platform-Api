using Model.Other;

namespace Model;

public class ResultHelper
{
    public static ApiResult Success(string message,object res)
    {
        return new ApiResult() { code = 0,message = message, data = res };
    }

    public static ApiResult Success(string message,object res,int total)
    {
        return new ApiResult() { code = 0,message = message, data = res ,total = total};
    }
    public static ApiResult Error(string message)
    {
        return new ApiResult() { code = 1, message = message };
    }
}