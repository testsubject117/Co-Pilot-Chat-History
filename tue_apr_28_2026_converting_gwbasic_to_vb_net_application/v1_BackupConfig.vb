Option Strict Off
Option Explicit On

Public Class BackupConfig
    ' Keep these configurable instead of hardcoding drive letters.
    Public Property WordSourceDir As String = "F:\WORD"
    Public Property WorkingDir As String = AppDomain.CurrentDomain.BaseDirectory.TrimEnd("\"c)
    Public Property BackupDir As String = "C:\BACKUP"

    ' Mirrors GWBASIC control files
    Public Property PackNumPath As String = "C:\PACK.NUM"
    Public Property NotesPath As String = "NOTES-4.ED"
    Public Property ErrorsListPath As String = "ERRORS.LST"

    ' Rotation settings
    Public Property PackMin As Integer = 1
    Public Property PackMax As Integer = 99
End Class