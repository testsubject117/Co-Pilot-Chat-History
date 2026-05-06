Option Strict Off
Option Explicit On

Imports System.IO
Imports System.IO.Compression
Imports System.Text

Public Class BackupService
    Private ReadOnly cfg As BackupConfig

    Public Sub New(cfg As BackupConfig)
        Me.cfg = cfg
    End Sub

    Public Function ReadAndIncrementPackNumber() As Integer
        Dim x As Integer = 0
        If File.Exists(cfg.PackNumPath) Then
            Dim txt = File.ReadAllText(cfg.PackNumPath).Trim()
            Integer.TryParse(New String(txt.Where(AddressOf Char.IsDigit).ToArray()), x)
        End If

        x += 1
        If x > cfg.PackMax OrElse x < cfg.PackMin Then x = cfg.PackMin

        ' Write back like WRITE#1,X
        Directory.CreateDirectory(Path.GetDirectoryName(cfg.PackNumPath))
        File.WriteAllText(cfg.PackNumPath, x.ToString() & Environment.NewLine)

        Return x
    End Function

    Public Sub WriteNotes(message As String)
        ' Original overwrote NOTES-4.ED in some cases; for backup error it overwrote too.
        ' Here we overwrite to match: WRITE#1, Q$
        File.WriteAllText(cfg.NotesPath, message & Environment.NewLine)
    End Sub

    Public Sub AppendError(message As String)
        Using sw As New StreamWriter(cfg.ErrorsListPath, append:=True)
            sw.WriteLine(message)
        End Using
    End Sub

    Public Sub UpdateZip(zipPath As String, files As IEnumerable(Of String), Optional baseDir As String = Nothing)
        Directory.CreateDirectory(Path.GetDirectoryName(zipPath))

        Using zip As ZipArchive = OpenOrCreateZip(zipPath)
            For Each filePath In files
                If String.IsNullOrWhiteSpace(filePath) Then Continue For
                If Not File.Exists(filePath) Then Continue For

                Dim entryName As String = GetEntryName(filePath, baseDir)
                ' Replace existing entry (ZIP -U behavior)
                Dim existing = zip.GetEntry(entryName)
                If existing IsNot Nothing Then existing.Delete()

                zip.CreateEntryFromFile(filePath, entryName, CompressionLevel.Optimal)
            Next
        End Using
    End Sub

    Public Sub UpdateZipFromPatterns(zipPath As String, searchRoot As String, patterns As IEnumerable(Of String), Optional recursive As Boolean = False)
        Dim optionDir As SearchOption = If(recursive, SearchOption.AllDirectories, SearchOption.TopDirectoryOnly)

        Dim allFiles As New List(Of String)()
        For Each pat In patterns
            If String.IsNullOrWhiteSpace(pat) Then Continue For
            If Not Directory.Exists(searchRoot) Then Continue For

            ' Handle "BILLS." (no extension) etc—treat as literal file if it contains no wildcard.
            If Not pat.Contains("*"c) AndAlso Not pat.Contains("?"c) Then
                Dim literal = Path.Combine(searchRoot, pat)
                If File.Exists(literal) Then allFiles.Add(literal)
            Else
                allFiles.AddRange(Directory.GetFiles(searchRoot, pat, optionDir))
            End If
        Next

        UpdateZip(zipPath, allFiles, baseDir:=searchRoot)
    End Sub

    Public Sub VerifyZip(zipPath As String)
        ' Replacement for "UNZIP -t": read each entry fully.
        Using fs As New FileStream(zipPath, FileMode.Open, FileAccess.Read, FileShare.Read)
            Using zip As New ZipArchive(fs, ZipArchiveMode.Read, leaveOpen:=False)
                For Each entry In zip.Entries
                    Using es = entry.Open()
                        Dim buffer(8191) As Byte
                        While True
                            Dim n = es.Read(buffer, 0, buffer.Length)
                            If n <= 0 Then Exit While
                        End While
                    End Using
                Next
            End Using
        End Using
    End Sub

    Public Function MakeBackupZipName(packNumber As Integer) As String
        Dim x$ = packNumber.ToString()
        Return Path.Combine(cfg.BackupDir, $"BACKUP{x$}.ZIP")
    End Function

    Private Function OpenOrCreateZip(zipPath As String) As ZipArchive
        If File.Exists(zipPath) Then
            Dim fs = New FileStream(zipPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
            Return New ZipArchive(fs, ZipArchiveMode.Update)
        Else
            Dim fs = New FileStream(zipPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None)
            Return New ZipArchive(fs, ZipArchiveMode.Update)
        End If
    End Function

    Private Function GetEntryName(filePath As String, baseDir As String) As String
        If String.IsNullOrWhiteSpace(baseDir) Then
            Return Path.GetFileName(filePath)
        End If

        Dim fullBase = Path.GetFullPath(baseDir).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) & Path.DirectorySeparatorChar
        Dim fullFile = Path.GetFullPath(filePath)

        If fullFile.StartsWith(fullBase, StringComparison.OrdinalIgnoreCase) Then
            Dim rel = fullFile.Substring(fullBase.Length)
            Return rel.Replace("\"c, "/"c)
        End If

        Return Path.GetFileName(filePath)
    End Function
End Class