# Shift.WinService
Runs Shift server inside a windows service app container.

To setup this app:
- Setup storage media of your choice, install Redis server or MS SQL server. Check out the [Shift Quick Start - Infrastructure Setup](https://github.com/hhalim/Shift/wiki/Quick-Start#infrastructure-setup) section in the wiki for more information.
- Open and compile project in Visual Studio. 
- Update storage configuration in App.config file. Refer to [Shift Server wiki](https://github.com/hhalim/Shift/wiki/Shift-Server) for more configuration options. 
- Open a Windows command prompt window with Administrator privilege.
- Go to the folder where Shift.WinService.exe is located.
- Run `Shift.WinService.exe -install` command to install the windows service. Use `-uninstall` to uninstall the service.
- Open Windows Services management console, locate the service based on the `ServiceName` in App.config. Start the service.

To send jobs to the Shift Windows service server, use the [Shift.Demo.Client](https://github.com/hhalim/Shift.Demo.Client) console app.
