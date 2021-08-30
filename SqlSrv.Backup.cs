using Ionic.Zip;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace SqlSrv.Backup
{
    public class Task
    {
        public bool Restore(SqlConnection Connection, string strDataBase, string strBackupPath, bool bolReplace = false, bool bolZip = false, string strDataPath = null, string strLogPath = null)
        {
            bool bolRes = false;
            try
            {
                if (bolZip)
                {
                    ZipFile zipBackup = ZipFile.Read(strBackupPath);
                    foreach (ZipEntry e in zipBackup.Where(x => x.FileName.ToLower().EndsWith(".bak")))
                    {
                        strBackupPath = Path.GetDirectoryName(strBackupPath) + @"\" + e.FileName;
                        e.Extract(Path.GetDirectoryName(strBackupPath), ExtractExistingFileAction.OverwriteSilently);
                    }
                }
                if (strDataPath == null || strLogPath == null)
                {
                    string strPath = "SELECT SERVERPROPERTY('instancedefaultdatapath') AS[datapath], SERVERPROPERTY('instancedefaultlogpath') AS[logpath]";
                    SqlCommand CmdPath = new SqlCommand(strPath, Connection);
                    CmdPath.Connection.Open();
                    SqlDataReader RdrPath = CmdPath.ExecuteReader();
                    while (RdrPath.Read())
                    {
                        strDataPath = strDataPath == null ? RdrPath["datapath"].ToString() + strDataBase + ".mdf" : strDataPath;
                        strLogPath = strLogPath == null ? RdrPath["logpath"].ToString() + strDataBase + "_log.ldf" : strLogPath;
                    }
                    CmdPath.Connection.Close();
                }
                string strDataName = string.Empty;
                string strLogName = string.Empty;
                string strFile = "RESTORE FILELISTONLY FROM DISK = N'" + strBackupPath + "'";
                SqlCommand CmdFile = new SqlCommand(strFile, Connection);
                CmdFile.Connection.Open();
                SqlDataReader RdrFile = CmdFile.ExecuteReader();
                while (RdrFile.Read())
                {
                    if (RdrFile["Type"].ToString() == "D")
                    {
                        strDataName = RdrFile["LogicalName"].ToString();
                    }
                    if (RdrFile["Type"].ToString() == "L")
                    {
                        strLogName = RdrFile["LogicalName"].ToString();
                    }
                }
                CmdFile.Connection.Close();
                string strRestore = "USE [master] RESTORE DATABASE[" + strDataBase + "] FROM DISK = N'" + strBackupPath + @"' WITH FILE = 1, MOVE N'" + strDataName + "' TO N'" + strDataPath + "', MOVE N'" + strLogName + "' TO N'" + strLogPath + "', NOUNLOAD,";
                strRestore += bolReplace ? " REPLACE, STATS = 5" : "STATS = 5";
                SqlCommand Cmd = new SqlCommand(strRestore, Connection);
                Cmd.Connection.Open();
                Cmd.ExecuteNonQuery();
                Cmd.Connection.Close();
                if (bolZip)
                {
                    File.Delete(strBackupPath);
                }
                bolRes = true;
                Connection.Close();

            }
            catch (Exception)
            {
                throw;
            }
            return bolRes;
        }

        public static bool BackUp(SqlConnection Connection, string strDataBase, string strFilePath, bool bolVer = false, bool bolZip = false, bool bolCheckSum = false)
        {
            bool bolRes = false;
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(strFilePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(strFilePath));
                }
                if (Path.GetExtension(strFilePath).ToLower() != ".bak")
                {
                    throw new Exception("The file extension needs be .bak");
                }
                string strQuery = "BACKUP DATABASE[" + strDataBase + @"] TO DISK = N'" + strFilePath + "' WITH NOFORMAT, NOINIT, NAME = N'" + strDataBase + "-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10";
                strQuery += bolCheckSum ? ", CHECKSUM" : string.Empty;
                if (bolVer)
                {
                    strQuery += @" declare @backupSetId as int select @backupSetId = position from msdb..backupset where database_name = N'" + strDataBase + "' and backup_set_id = (select max(backup_set_id) from msdb..backupset where database_name = N'" + strDataBase + @"') if @backupSetId is null begin raiserror(N'Verify failed. Backup information for database ''" + strDataBase + @"'' not found.', 16, 1) end RESTORE VERIFYONLY FROM  DISK = N'" + strFilePath + @"' WITH FILE = @backupSetId, NOUNLOAD, NOREWIND";
                }
                SqlCommand Cmd = new SqlCommand(strQuery, Connection);
                Cmd.Connection.Open();
                Cmd.ExecuteNonQuery();
                Cmd.Connection.Close();
                if (bolZip)
                {
                    using (ZipFile zipBackup = new ZipFile())
                    {
                        zipBackup.AddFile(strFilePath, ".");
                        zipBackup.Save(strFilePath + ".zip");
                    }
                    File.Delete(strFilePath);
                }
                bolRes = true;
                Connection.Close();

            }
            catch (SqlException ex)
            {
                if (ex.Number == 911)
                {
                    throw new Exception("The database dosen't exists");
                }
                else
                {
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return bolRes;
        }
    }
}
