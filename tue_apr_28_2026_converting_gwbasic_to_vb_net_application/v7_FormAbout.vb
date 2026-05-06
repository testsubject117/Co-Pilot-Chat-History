Option Strict On
Option Explicit On

Imports System.Diagnostics
Imports System.Text

Partial Public Class FormAbout

    Private Sub FormAbout_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "About AMiOffice"

        lblAppName.Text = "AMiOffice"
        lblAbout.Text = "About: " & BuildInfo.DisplayVersion
        lblDevelopedBy.Text = "Developed By: Kirk Saffell"

        Dim exePath As String = Application.ExecutablePath
        Dim fvi As FileVersionInfo = FileVersionInfo.GetVersionInfo(exePath)

        Dim sb As New StringBuilder()
        sb.AppendLine(BuildInfo.DisplayVersion)
        sb.AppendLine()
        sb.AppendLine($"Assembly Version : {My.Application.Info.Version}")
        sb.AppendLine($"File Version     : {Safe(fvi.FileVersion)}")
        sb.AppendLine($"Product Version  : {Safe(fvi.ProductVersion)}")
        sb.AppendLine()
        sb.AppendLine($"Product Name     : {Safe(fvi.ProductName, "AMiOffice")}")
        sb.AppendLine($"Company          : {Safe(fvi.CompanyName)}")
        sb.AppendLine($"Copyright        : {Safe(fvi.LegalCopyright)}")
        sb.AppendLine()
        sb.AppendLine($"Executable Path  : {exePath}")

        rtbInfo.Text = sb.ToString()
    End Sub

    Private Shared Function Safe(value As String, Optional fallback As String = "") As String
        If String.IsNullOrWhiteSpace(value) Then
            If String.IsNullOrWhiteSpace(fallback) Then
                Return "(not set)"
            End If
            Return fallback
        End If
        Return value.Trim()
    End Function

End Class