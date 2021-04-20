cd ..
cd Valuator
dotnet build
cd ..
cd RankCalculator
dotnet build
cd ..
cd EventsLogger
dotnet build

cd ..
cd Valuator
start dotnet run --no-build --urls "http://localhost:5001/"
start dotnet run --no-build --urls "http://localhost:5002/"

cd ..
cd RankCalculator
start dotnet run --no-build
start dotnet run --no-build

cd ..
cd EventsLogger
start dotnet run --no-build

cd ..

cd nginx
start nginx.exe
