# Sử dụng .NET 8 SDK để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["DoAnCSharp.AdminWeb.csproj", "./"]
RUN dotnet restore "DoAnCSharp.AdminWeb.csproj"

# Copy toàn bộ source code
COPY . .
RUN dotnet build "DoAnCSharp.AdminWeb.csproj" -c Release -o /app/build

# Publish ứng dụng
FROM build AS publish
RUN dotnet publish "DoAnCSharp.AdminWeb.csproj" -c Release -o /app/publish

# Sử dụng runtime image để chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose cổng cho Render (Render tự động map PORT environment variable)
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE $PORT

ENTRYPOINT ["dotnet", "DoAnCSharp.AdminWeb.dll"]
