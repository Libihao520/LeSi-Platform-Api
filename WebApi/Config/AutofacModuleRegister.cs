using System.Reflection;
using Autofac;
using CommonUtil;
using CommonUtil.AiGcUtil;

namespace WebApi.Config;

public class AutofacModuleRegister : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        //通过反射注册接口和实现之间的关系
        Assembly interfaceAssembly = Assembly.Load("Interface");
        Assembly serviceAssembly = Assembly.Load("Service");
        builder.RegisterAssemblyTypes(interfaceAssembly, serviceAssembly).AsImplementedInterfaces();
        //ai请求策略注入
        builder.RegisterType<AiRequestProcessor>().AsSelf().SingleInstance();
        //Ai策略工厂注入
        builder.RegisterType<AiRequestStrategyFactory>().AsSelf().SingleInstance();
        //讯飞API实现注册
        builder.RegisterType<SparkAiRequestStrategy>().AsSelf().InstancePerDependency();
        //DeepSeek-API实现注册
        builder.RegisterType<DeepSeekAiRequestStrategy>().AsSelf().InstancePerDependency();
        //发送邮箱工具
        builder.RegisterType<EmailUtil>().AsSelf().SingleInstance();
        //获取用户信息工具
        builder.RegisterType<UserInformationUtil>().AsSelf().SingleInstance();
    }
}