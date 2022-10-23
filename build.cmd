dotnet restore src/TargetFunction

dotnet lambda package -pl src/TargetFunction --configuration Release --framework net6.0 --output-package src/TargetFunction/bin/Release/net6.0/TargetFunction.zip