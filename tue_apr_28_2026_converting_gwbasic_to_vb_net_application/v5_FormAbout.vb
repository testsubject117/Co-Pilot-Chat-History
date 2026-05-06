Option Strict On
Option Explicit On

Imports System.Diagnostics
Imports System.Reflection
Imports System.Text

Public Class FormAbout

    Private Sub FormAbout_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Title bar text
        Me.Text = "About AMiOffice"

        ' Big red app name line (your designer label)
        lblAppName.Text = "AMiOffice"

        ' Optional: make the "About:" header include the build display
        lblAbout.Text = "About: " & BuildInfo.DisplayVersion

        ' Show detailed info in the rich text box
        rtbInfo.Clear()
        rtbInfo.SelectionAlignment = HorizontalAlignment.Left
        rtbInfo.Font = New Font("Consolas", 9.75F, FontStyle.Regular)

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

        ' If you want a nicer "Developed By" line, set it here:
        ' (Change the name to whatever you want displayed.)
        lblDevelopedBy.Text = "Developed By: Kirk Saffell"

        ' If you want to show an embedded image in picDeveloper, do it here.
        ' (Only if you have one available. Otherwise leave it blank.)
        ' picDeveloper.Image = My.Resources.YourImageName
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