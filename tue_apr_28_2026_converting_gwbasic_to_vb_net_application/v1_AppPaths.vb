Option Strict Off
Option Explicit On

Imports System.IO

Public Module AppPaths

    ' Base folder for this application's files on the Invoice server
    Public ReadOnly Property AppRoot As String = "\\invoice\MainMenu"

    ' All legacy/root files should live here going forward
    Public ReadOnly Property DataDir As String = Path.Combine(AppRoot, "Data")

    ' Zip outputs go here
    Public ReadOnly Property BackupDir As String = Path.Combine(AppRoot, "Backups")

    ' Word docs live outside the app root per your note
    Public ReadOnly Property WordDocsDir As String = "\\invoice\word"

End Module