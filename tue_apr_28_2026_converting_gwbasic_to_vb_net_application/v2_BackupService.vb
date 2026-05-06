Option Strict Off
Option Explicit On

Imports System
Imports System.IO
Imports System.IO.Compression
Imports System.Linq

Public Module BackupService

    ' Implements (I) Backup Price List & Rolodex in .NET.
    ' - Sources:
    '     \\invoice\MainMenu\Data\  (data files)
    '     \\invoice\word\*.doc      (word docs)
    ' - Outputs:
    '     \\invoice\MainMenu\Backups\WORD.zip
    '     \\invoice\MainMenu\Backups\BACKUP##.zip (rotating via PACK.NUM)

    Public Sub RunBackupPriceListAndRolodex()
        Directory.CreateDirectory(AppPaths.DataDir)
        Directory.CreateDirectory(AppPaths.BackupDir)

        ' 1) Backup Word docs to WORD.zip
        Dim wordZip As String = Path.Combine(AppPaths.BackupDir, "WORD.zip")
        CreateZipFromPattern(wordZip, AppPaths.WordDocsDir, "*.doc", SearchOption.TopDirectoryOnly)

        ' 2) Rotating backup for DataDir
        Dim nextNum As Integer = GetNextPackNumberAndAdvance(Path.Combine(AppPaths.DataDir, "PACK.NUM"))
        Dim backupZip As String = Path.Combine(AppPaths.BackupDir, $"BACKUP{nextNum}.zip")

        CreateZipForData(backupZip, AppPaths.DataDir)
    End Sub

    Private Sub CreateZipFromPattern(zipPath As String, folder As String, pattern As String, opt As SearchOption)
        If Not Directory.Exists(folder) Then
            ' If the Word share doesn't exist, still create an empty zip so the process is predictable.
            CreateEmptyZip(zipPath)
            Return
        End If

        Dim files = Directory.GetFiles(folder, pattern, opt)

        Using fs As New FileStream(zipPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
            Using archive As New ZipArchive(fs, ZipArchiveMode.Create)
                For Each f In files
                    Dim entryName As String = Path.GetFileName(f)
                    archive.CreateEntryFromFile(f, entryName, CompressionLevel.Optimal)
                Next
            End Using
        End Using
    End Sub

    Private Sub CreateZipForData(zipPath As String, dataDir As String)
        If Not Directory.Exists(dataDir) Then
            CreateEmptyZip(zipPath)
            Return
        End If

        ' Match the spirit of the old backup: key business files in the data root.
        ' (No recursion by default.)
        Dim includePatterns As String() = {
            "*.DAT",
            "*.CUR",
            "PRC*.PRC",
            "PHONE*.LST",
            "BILLS.*",
            "PACK.NUM"
        }

        Dim files As New System.Collections.Generic.List(Of String)()

        For Each pat In includePatterns
            Try
                files.AddRange(Directory.GetFiles(dataDir, pat, SearchOption.TopDirectoryOnly))
            Catch
                ' ignore
            End Try
        Next

        ' De-dupe
        files = files.Distinct(StringComparer.OrdinalIgnoreCase).ToList()

        Using fs As New FileStream(zipPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
            Using archive As New ZipArchive(fs, ZipArchiveMode.Create)
                For Each f In files
                    Dim entryName As String = Path.GetFileName(f)
                    archive.CreateEntryFromFile(f, entryName, CompressionLevel.Optimal)
                Next
            End Using
        End Using
    End Sub

    Private Sub CreateEmptyZip(zipPath As String)
        Using fs As New FileStream(zipPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
            Using archive As New ZipArchive(fs, ZipArchiveMode.Create)
                ' no entries
            End Using
        End Using
    End Sub

    ' PACK.NUM behavior:
    ' - Stores a number (1..99)
    ' - Each run returns current value and increments it
    ' - Rolls over after 99 back to 1
    Private Function GetNextPackNumberAndAdvance(packNumPath As String) As Integer
        Dim n As Integer = 1

        Try
            If File.Exists(packNumPath) Then
                Dim txt As String = File.ReadAllText(packNumPath).Trim()
                If txt <> "" Then
                    Integer.TryParse(txt, n)
                End If
            End If
        Catch
            n = 1
        End Try

        If n < 1 OrElse n > 99 Then n = 1

        Dim nextN As Integer = n + 1
        If nextN > 99 Then nextN = 1

        Try
            File.WriteAllText(packNumPath, nextN.ToString())
        Catch
            ' If we can't write, still return n so backup can proceed.
        End Try

        Return n
    End Function

End Module