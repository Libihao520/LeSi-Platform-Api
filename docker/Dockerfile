# 使用官方镜像（阿里云海外构建会自动加速）
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制解决方案和项目文件
COPY ["LeSi-Platform-Api.sln", "./"]
COPY ["WebApi/WebApi.csproj", "WebApi/"]
COPY ["CommonUtil/CommonUtil.csproj", "CommonUtil/"]
COPY ["Model/Model.csproj", "Model/"]
COPY ["Service/Service.csproj", "Service/"]
COPY ["EFCoreMigrations/EFCoreMigrations.csproj", "EFCoreMigrations/"]
COPY ["Interface/Interface.csproj", "Interface/"]

# 还原依赖
RUN dotnet restore "LeSi-Platform-Api.sln"

# 复制所有代码并发布
COPY . .
RUN dotnet publish "WebApi/WebApi.csproj" -c Release -o /app/publish

# 运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "WebApi.dll"]