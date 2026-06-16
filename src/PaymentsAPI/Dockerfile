# Stage 1: Build the application using the .NET 8 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy project file and restore native dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy all remaining source files and compile package
COPY . ./
RUN dotnet publish -c Release -o out

# Stage 2: Create runtime container image for AWS Fargate environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

EXPOSE 80
EXPOSE 443

USER 1000

ENTRYPOINT ["dotnet", "PaymentsAPI.dll"]
