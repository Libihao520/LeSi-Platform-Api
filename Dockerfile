# 构建阶段
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. 复制项目文件（利用Docker缓存机制）
COPY ["LeSi-Platform-Api.sln", "./"]
COPY ["WebApi/WebApi.csproj", "WebApi/"]
COPY ["CommonUtil/CommonUtil.csproj", "CommonUtil/"]
COPY ["Model/Model.csproj", "Model/"]
COPY ["Service/Service.csproj", "Service/"]
COPY ["EFCoreMigrations/EFCoreMigrations.csproj", "EFCoreMigrations/"]
COPY ["Interface/Interface.csproj", "Interface/"]

# 2. 还原依赖（仅在项目文件变更时重新执行）
RUN dotnet restore "LeSi-Platform-Api.sln"

# 3. 复制源代码
COPY . .

# 4. 发布项目（指定Release配置，禁用AppHost）
RUN dotnet publish "WebApi/WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 运行阶段
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
# 使用非root用户运行，增强安全性
USER $APP_UID
WORKDIR /app

# 复制发布产物
COPY --from=build /app/publish .

# 暴露端口
EXPOSE 5157

# 入口点
ENTRYPOINT ["dotnet", "WebApi.dll"]