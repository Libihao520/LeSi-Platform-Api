#基于 `microsoft/dotnet:1.0.0-core` 来构建我们的镜像
FROM mcr.microsoft.com/dotnet/aspnet:6.0


#设置工作目录为 `/publish` 文件夹，即容器启动默认的文件夹
WORKDIR /publish
#拷贝项目publish文件夹中的所有文件到 docker容器中的publish文件夹中 
COPY /publish /publish

# 监听5157
ENV ASPNETCORE_URLS=http://+:5157

#设置Docker容器对外暴露8003端口
EXPOSE 5157

#使用`dotnet testmvc.dll`来运行应用程序

CMD ["dotnet", "WebApi.dll", "--server.urls", "http://*:5157"]