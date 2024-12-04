# Challenge

Challenge is an application that allows backup in separated csv files specific data from all devices. It allows to get new updates every specific time.


## Prerequisites

The sample application requires:

- [.Net core 2.0 SDK](https://dot.net/core) or higher

Challenge application connects to the MyGeotab cloud hosting services, please ensure that devices have been registered and added to the database. The following information is required:

- Username
- Password
- Database (customer)
- Server (my.geotab.com)


## Getting Started

```shell
> gh repo clone aljimenez1986/Challenge
> cd challenge
> dotnet run 
```

The application will bring up the following console:

```shell
" Command line parameters:"

> dotnet run <path> <username> <password> <database> <server> <delay>
 Example: dotnet.run path username database server delay

 path       - Path to create files (Example: c:\)
 username   - Geotab user name
 password   - Geotab password
 database   - Database name (Example: G560)
 server     - Sever host name (Example: my.geotab.com)
 delay      - (optional, 60000 by default) Time to search for changes in miliseconds.