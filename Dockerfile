FROM microsoft/dotnet:1.1.0-sdk-projectjson

RUN apt-get update
RUN wget -qO- https://deb.nodesource.com/setup_4.x | bash -
RUN apt-get install -y build-essential nodejs


COPY . /app


WORKDIR /app/src/LanBackup.WebApp
RUN ["dotnet", "restore"]

WORKDIR /app/src/LanBackup.DataCore
RUN ["dotnet", "restore"]

WORKDIR /app/src/LanBackup.ModelsCore
RUN ["dotnet", "restore"]



WORKDIR /app/src/LanBackup.WebApp
RUN ["dotnet", "build"]

EXPOSE 5000/tcp

CMD ["dotnet", "run", "--server.urls", "http://*:5000"]