# Runs the app from the artifact the SequensisCi builder already published to
# artifacts/images/{assembly_name}. The builder configures the private Azure
# DevOps NuGet feed, restores (incl. private packages) and publishes the app —
# this image just runs the output. Same pattern as accountmanagementservice et
# al., so no NuGet feed / nuget.config auth is needed inside the Docker build.

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

ARG assembly_name

# Org-internal Sectigo root/intermediate CA certs (public certs, not secrets —
# same ones accountmanagementservice trusts) so this service can call other
# internal org HTTPS endpoints.
COPY certs/ /usr/local/share/ca-certificates/
RUN update-ca-certificates

# Pre-published app output from the SequensisCi build stage.
COPY artifacts/images/${assembly_name}/. /app

ENV _exe="${assembly_name}.dll"
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT exec dotnet "$_exe"
