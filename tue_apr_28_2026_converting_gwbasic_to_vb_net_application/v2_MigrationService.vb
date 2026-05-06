Option Strict Off
Option Explicit On

Imports System.IO

Public Module MigrationService

    Private ReadOnly LegacyRoot As String = "\\invoice"

    Public Sub EnsureFoldersAndMigrateOnce()
        Directory.CreateDirectory(AppPaths.AppRoot)
        Directory.CreateDirectory(AppPaths.DataDir)
        Directory.CreateDirectory(AppPaths.BackupDir)

        Dim patterns As String() = {
            "*.DAT", "*.CUR", "*.LST", "*.PRC",
            "PACK.NUM", "BILLS.*", "*.CHK", "*.TXT"
        }

        For Each pattern In patterns
            Dim files As String()

            Try
                files = Directory.GetFiles(LegacyRoot, pattern, SearchOption.TopDirectoryOnly)
            Catch
                Continue For
            End Try

            For Each src In files
                Dim dest As String = Path.Combine(AppPaths.DataDir, Path.GetFileName(src))

                ' Skip if already migrated
                If File.Exists(dest) Then Continue For

                Try
                    File.Move(src, dest)
                Catch
                    ' ignore (in use / permissions / etc.)
                End Try
            Next
        Next
    End Sub

End Module