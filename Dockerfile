FROM node:24 AS npm-stage

WORKDIR /app

COPY UI/Web/package.json UI/Web/package-lock.json ./
RUN npm ci

COPY UI/Web ./

RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS dotnet-stage

WORKDIR /InOut

COPY API/*.csproj ./

RUN dotnet restore

COPY API/ ./

RUN dotnet publish ./API.csproj -c Release -o /API/

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /InOut

COPY --from=npm-stage /app/dist/Web/browser/ /InOut/wwwroot
COPY --from=dotnet-stage /API/ /InOut

CMD [ "/InOut/In-Out" ]