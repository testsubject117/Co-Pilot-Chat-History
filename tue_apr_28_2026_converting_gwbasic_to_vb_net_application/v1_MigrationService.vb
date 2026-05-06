Option Strict Off
Option Explicit On

Imports System
Imports System.IO
Imports System.Linq

Public Module MigrationService

    ' Legacy location (root of the share). Files used to be dumped here.
    Private ReadOnly LegacyRoot As String = "\\invoice"

    ' MigrationRule = Skip (per your decision)
    Public Sub EnsureFoldersAndMigrateOnce()
        Directory.CreateDirectory(AppPaths.AppRoot)
        Directory.CreateDirectory(AppPaths.DataDir)
        Directory.CreateDirectory(AppPaths.BackupDir)

        ' Move a known set of legacy file patterns from \\invoice\ to \\invoice\MainMenu\Data\
        ' Skip if destination already exists.
        Dim patterns As String() = {
            "*.DAT",
            "*.CUR",
            "*.LST",
            "*.PRC",
            "PACK.NUM",
            "BILLS.*",
            "*.CHK",
            "*.TXT"
        }

        For Each pattern In patterns
            Dim files As String() = Array.Empty(Of String)()

            Try
                files = Directory.GetFiles(LegacyRoot, pattern, SearchOption.TopDirectoryOnly)
            Catch
                ' If the share root isn't accessible or pattern fails, just continue.
                Continue For
            End Try

            For Each src In files
                Dim fileName As String = Path.GetFileName(src)
                Dim dest As String = Path.Combine(AppPaths.DataDir, fileName)

                ' Skip rule
                If File.Exists(dest) Then
                    Continue For
                End If

                ' Don't try to move our own folder if someone created files weirdly
                If String.Equals(src.TrimEnd("\"c), dest.TrimEnd("\"c), StringComparison.OrdinalIgnoreCase) Then
                    Continue For
                End If

                Try
                    File.Move(src, dest)
                Catch
                    ' If file is in use or permissions prevent move, skip.
                End Try
            Next
        Next
    End Sub

End Module