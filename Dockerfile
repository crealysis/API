# Stage 1: Build the application
# This stage uses the full .NET SDK image to build and publish the application.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory inside the container
WORKDIR /src

# Copy the project files to the container. The .csproj file is copied first to leverage
# Docker's build cache. If only the project file changes, the dependencies
# won't need to be restored again.
# The project file is named 'API.csproj' as per your provided information.
COPY ["API.csproj", "./"]

# Restore the NuGet packages.
RUN dotnet restore "API.csproj"

# Copy the rest of the source code and build the application.
COPY . .

# Change the working directory to the API project
WORKDIR "/src"

# Publish the application for a release build.
# The output is placed in the /app/publish directory.
RUN dotnet publish "API.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Create the final production image
# This stage uses a much smaller, runtime-only image. It's more secure and lightweight.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Set the working directory for the final image
WORKDIR /app

# Expose the port that the application listens on.
# ASP.NET Core apps listen on port 8080 by default with .NET 8+.
EXPOSE 8080

# Copy the published application from the `publish` stage to the final image.
# We only copy the necessary files, not the entire SDK or source code.
COPY --from=build /app/publish .

# Define the entry point for the container. This command is executed when the container starts.
# The entry point is the published DLL, 'API.dll'.
ENTRYPOINT ["dotnet", "API.dll"]
