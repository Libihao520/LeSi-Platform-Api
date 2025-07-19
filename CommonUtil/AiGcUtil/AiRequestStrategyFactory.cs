using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Model.Options;

namespace CommonUtil.AiGcUtil;

/// <summary>
/// 策略工厂
/// </summary>
public class AiRequestStrategyFactory
{
    private readonly IServiceProvider _serviceProvider;


    public AiRequestStrategyFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IAiRequestStrategy Create(string strategyType)
    {
        return strategyType switch
        {
            "Spark" => _serviceProvider.GetRequiredService<SparkAiRequestStrategy>(),
            "DeepSeek" => _serviceProvider.GetRequiredService<DeepSeekAiRequestStrategy>(),
            _ => throw new ArgumentException("Invalid strategy type")
        };
    }
}