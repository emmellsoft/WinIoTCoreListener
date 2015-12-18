@echo. Note that it must be a RELEASE build!
@echo.
@echo.
nuget pack "..\WinIoTCoreListener.Lib\WinIoTCoreListener.Lib.csproj" -Symbols -IncludeReferencedProjects -Prop Configuration=Release
@echo.
@echo.
@echo After a successful NuGet package build:
@echo.
@echo First execute:
@echo nuget setApiKey {API KEY}
@echo Where the {API KEY} is gotten from https://www.nuget.org/account.
@echo.
@echo Then execute:
@echo nuget push WinIoTCoreListener.xxxxxxxx.nupkg
