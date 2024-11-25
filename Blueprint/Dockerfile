#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 

ARG ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT
ENV ASPNETCORE_URLS=http://+:8080
RUN groupadd -r -g 1001 usergroup && useradd -r -u 1001 -g usergroup userapp
COPY . /app
RUN chown -R userapp:usergroup /app
WORKDIR /app
USER userapp
EXPOSE 8080

ENTRYPOINT ["dotnet", "Blueprint.dll"]