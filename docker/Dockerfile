# 使用官方 .NET 8 SDK 镜像
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. 复制解决方案和所有.csproj文件（利用缓存）
COPY ["LeSi-Platform-Api.sln", "./"]
COPY ["WebApi/WebApi.csproj", "WebApi/"]
COPY ["CommonUtil/CommonUtil.csproj", "CommonUtil/"]
COPY ["Model/Model.csproj", "Model/"]
COPY ["Service/Service.csproj", "Service/"]
COPY ["EFCoreMigrations/EFCoreMigrations.csproj", "EFCoreMigrations/"]
COPY ["Interface/Interface.csproj", "Interface/"]

# 2. 还原依赖
RUN dotnet restore "LeSi-Platform-Api.sln"

# 3. 复制剩余所有代码
COPY . .

# 4. 发布WebApi项目
RUN dotnet publish "WebApi/WebApi.csproj" -c Release -o /app/publish

# 5. 使用运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "WebApi.dll"]