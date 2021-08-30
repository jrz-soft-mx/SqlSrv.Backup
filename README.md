# SqlSrv.Backup
Tool for Backup and Restore Microsoft Sql Server Databases and compress the backup

## The NuGet Package

````powershell
PM> Install-Package SqlSrv.Backup -Version 1.1.0
````

## How it works

Backup and recover your database only call with connection, database name, backup file path, addicionaly for backup your can compress the file, verifly the file and checksum. for recover yor can set zip for compresed files, and replace database, Data File Path and Log File Path

### Fast Backup database without Compression
Example for Fast Backup database without Compression

````csharp
SqlConnection Con = new SqlConnection(@"Data Source=LOCALHOST\SQL2012; User id=sa; Password=MyPwd");
string strDataBase = "MyDataBase";
string strFile = @"C:\backup\" + strDataBase + "_" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".bak";
SqlSrv.Backup.Task.BackUp(Con, strDataBase, strFile);
````

### Fast Recover Database without compression
Example for Fast recover database without Compression

````csharp
SqlConnection Con = new SqlConnection(@"Data Source=LOCALHOST\SQL2012; User id=sa; Password=MyPwd");
string strDataBase = "MyDataBase_New";
string strFileBackup = @"C:\backup\MyDataBase_20210830T102729.bak";
SqlSrv.Backup.Task.Restore(Con, strDataBase, strFileBackup);
````

### Backup database with Compression, verifly and checksum
Example for Backup database with Compression, verifly and checksum

````csharp
SqlConnection Con = new SqlConnection(@"Data Source=LOCALHOST\SQL2012; User id=sa; Password=MyPwd");
string strDataBase = "MyDataBase";
string strFile = @"C:\backup\" + strDataBase + "_" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".bak";
//BackUp(Connection, strDataBase, strFilePath, bolVer = false, bolZip = false, bolCheckSum = false)
SqlSrv.Backup.Task.BackUp(Con, strDataBase, strFile, true, true, true);
````

### Recover Database with compression and custom Data and Log File Path
Example for recover database with Compression and custom data and log file path

````csharp
SqlConnection Con = new SqlConnection(@"Data Source=LOCALHOST\SQL2012; User id=sa; Password=MyPwd");
string strDataBase = "MyDataBase_New";
string strFileBackup = @"C:\backup\MyDataBase_20210830T102729.bak.zip";
string strDataFile = @"C:\backup\MyDataBase_New.mdf";
string strLogFile = @"C:\backup\MyDataBase_New_Log.ldf";
//Restore(Connection, strDataBase, strBackupPath, bolReplace = false, bolZip = false, strDataFilePath = null, strLogFilePath = null)
SqlSrv.Backup.Task.Restore(Con, strDataBase, strFileBackup, true, true, strDataFile, strLogFile);
````
