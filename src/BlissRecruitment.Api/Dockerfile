FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/BlissRecruitment.Api/BlissRecruitment.Api.csproj", "src/BlissRecruitment.Api/"]
RUN dotnet restore "src/BlissRecruitment.Api/BlissRecruitment.Api.csproj"
COPY . .
WORKDIR "/src/src/BlissRecruitment.Api"
RUN dotnet build "BlissRecruitment.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlissRecruitment.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlissRecruitment.Api.dll"]
