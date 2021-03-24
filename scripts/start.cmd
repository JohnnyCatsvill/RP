cd ..
cd Valuator
start dotnet run --urls "http://localhost:5001/"
start dotnet run --urls "http://localhost:5002/"

cd ..
cd RankCalculator
start dotnet run
start dotnet run

cd ..
cd EventsLogger
start dotnet run

cd ..
cd ..

start nats\nats-server.exe

cd nginx
start nginx.exe
