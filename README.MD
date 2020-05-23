## Что надо исправить:
Обработку ошибки когда у приложения нет прав на открытие порта
Когда нет активных соединений не гонять впустую сбор данных о процесах


## App

`Attention: his application is a test task, so you should not use it in product development`

### Description:

The application presents an analogue of the top utility from Unix systems and was created on `C#`and `dotnet core 3.1`.
The server part represents a program to which you can connect via the Web socket protocol and start receiving notifications about processes running on the server side.

Also, a console client has been added to the repository with the help of which you can connect to the server and start receiving data.



## Installation:

##### Clone repository

```shell
git clone https://github.com/aglebov-dev/web-sockets.git
```

##### Build solution with Docker

```shell
docker build -t ws .
```

##### Run server

```shell
docker run --name ws-server --rm -p 8085:8085 ws
```



### Connect to server

You can use the client, which is included in the repository, after completing its assembly.

```shell
dotnet build ./src
```

To start the client, pass the server connection address as an argument, like this

```shell
cd ./src/WSClient/bin/Debug/netcoreapp3.1
WSClient.exe ws://localhost:8085
```

Or, connect to server by web-socket from any one application.

After connecting through the channel, messages will start to come to you, for example, like this

```json
[
    {
        "Pid": 1,
        "User": "Admin",
        "Cpu": 64.24,
        "Memory": 5921440,
        "Time": "00:16:01.1799921",
        "Name": "Admin panel"
    }
]
```

