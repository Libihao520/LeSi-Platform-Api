using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using QRCoder;
using Interface;

public class GjxService : IGjxService
{
    public async Task<string> GetEwm(string txt)
    {
        // 创建二维码生成器
        var qrCodeGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrCodeGenerator.CreateQrCode(txt, QRCodeGenerator.ECCLevel.H);
        QRCode qrCode = new QRCode(qrCodeData);

        // 设置每个模块的像素大小
        int moduleSize = 15; // 每个模块的像素大小，可以根据需要调整

        // 计算图像的宽度和高度
        int qrCodeSize = qrCodeData.ModuleMatrix.Count * moduleSize;

        // 使用ImageSharp创建图像
        using (var bitmap = new Image<Rgba32>(qrCodeSize, qrCodeSize))
        {
            // 填充二维码数据
            for (int x = 0; x < qrCodeData.ModuleMatrix.Count; x++)
            {
                for (int y = 0; y < qrCodeData.ModuleMatrix.Count; y++)
                {
                    // 获取当前模块的颜色
                    Rgba32 moduleColor = qrCodeData.ModuleMatrix[x][y] ? Color.Black : Color.White;

                    // 填充模块区域
                    for (int i = 0; i < moduleSize; i++)
                    {
                        for (int j = 0; j < moduleSize; j++)
                        {
                            bitmap[x * moduleSize + i, y * moduleSize + j] = moduleColor;
                        }
                    }
                }
            }

            // 将图像保存到内存流中
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, new PngEncoder());
                memoryStream.Position = 0;
                byte[] byteImage = memoryStream.ToArray();
                string base64String = Convert.ToBase64String(byteImage);
                return "data:image/png;base64," + base64String;
            }
        }
    }
}