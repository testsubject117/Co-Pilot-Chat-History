Option Strict On
Option Explicit On

Imports System.Diagnostics
Imports System.Reflection

Public Module BuildInfo

    Public Function AssemblyVersionString() As String
        Dim v = Assembly.GetExecutingAssembly().GetName().Version
        Return If(v Is Nothing, "", v.ToString())
    End Function

    Public Function FileVersionString() As String
        Dim fvi = FileVersionInfo.GetVersionInfo(Application.ExecutablePath)
        Return If(fvi Is Nothing, "", fvi.FileVersion)
    End Function

    Public Function BuildDateTime() As DateTime
        ' Uses the EXE last write time (updates on rebuild/publish)
        Dim exePath = Assembly.GetExecutingAssembly().Location
        Return IO.File.GetLastWriteTime(exePath)
    End Function

End Module