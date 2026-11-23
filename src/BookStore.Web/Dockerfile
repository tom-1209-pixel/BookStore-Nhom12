# Dung chung: build context la thu muc src (xem docker-compose.yml)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish BookStore.Web/BookStore.Web.csproj -c Release -o /app --nologo

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
# Anh bia mau; thu muc nay duoc gan volume dung chung voi Api (xem docker-compose.yml)
COPY --from=build /src/AnhBia /data/anh-bia
ENV AnhBia__ThuMuc=/data/anh-bia
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "BookStore.Web.dll"]
