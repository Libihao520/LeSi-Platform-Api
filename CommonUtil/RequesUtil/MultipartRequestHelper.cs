namespace CommonUtil.RequesUtil;

public static class MultipartRequestHelper
{
    // This method checks if the Content-Type header has a multipart/form-data media type.
    public static bool HasFormFileContentDisposition(string contentType)
    {
        return !string.IsNullOrEmpty(contentType) && contentType.StartsWith("multipart/form-data");
    }
}