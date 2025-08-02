# 使用阿里云镜像加速（国内构建更快）
FROM registry.cn-hangzhou.aliyuncs.com/microsoft/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. 优先复制解决方案文件和所有.csproj文件（利用Docker缓存）
COPY ["LeSi-Platform-Api.sln", "./"]
COPY ["WebApi/WebApi.csproj", "WebApi/"]
COPY ["CommonUtil/CommonUtil.csproj", "CommonUtil/"]
COPY ["Model/Model.csproj", "Model/"]
COPY ["Service/Service.csproj", "Service/"]
COPY ["EFCoreMigrations/EFCoreMigrations.csproj", "EFCoreMigrations/"]
COPY ["Interface/Interface.csproj", "Interface/"]

# 2. 还原所有项目的NuGet包（依赖不变时跳过耗时步骤）
RUN dotnet restore "LeSi-Platform-Api.sln"

# 3. 复制剩余所有源代码
COPY . .

# 4. 发布WebApi项目（自动处理依赖的类库）
RUN dotnet publish "WebApi/WebApi.csproj" -c Release -o /app/publish

# 5. 使用运行时镜像
FROM registry.cn-hangzhou.aliyuncs.com/microsoft/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "WebApi.dll"]