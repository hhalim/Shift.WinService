# Shift.WinService
Runs Shift server inside a windows service app container.

To install this app:
- Open and compile project in Visual Studio. Update database and cache configuration in App.config.
- Open a command prompt window with Administrator privilege.
- Go to the folder where Shift.WinService.exe is located.
- Run `Shift.WinService.exe -install` to install the windows service. Use `-uninstall` to uninstall the service.
- Open Windows Services management console, locate the service based on the `ServiceName` in App.config. Start the service.

Use the [Shift.Demo.Client](https://github.com/hhalim/Shift.Demo.Client) to send jobs to the windows service server. 
