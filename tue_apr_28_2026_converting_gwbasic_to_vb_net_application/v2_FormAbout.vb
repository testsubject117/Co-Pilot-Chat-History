Option Strict Off
Option Explicit On

Imports System
Imports System.Drawing
Imports System.Windows.Forms

Public Class FormAbout

    Private Sub FormAbout_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Title labels are handled by the designer now:
        '   lblAbout  => "About:"
        '   lblAppName => app name (DarkRed)
        '   lblDevelopedBy => "Developed By:"

        ' If you want to adjust these at runtime, you can:
        ' lblAppName.Text = "Active Magnetic Inspection Main Menu Application"

        ' Populate the info box (edit as desired)
        rtbInfo.Clear()
        rtbInfo.AppendText("Active Magnetic Inspection Main Menu Application" & Environment.NewLine)
        rtbInfo.AppendText(Environment.NewLine)
        rtbInfo.AppendText("Main Menu port / modernization UI." & Environment.NewLine)
        rtbInfo.AppendText(Environment.NewLine)
        rtbInfo.AppendText("Data folders:" & Environment.NewLine)
        rtbInfo.AppendText("  AppRoot:   " & AppPaths.AppRoot & Environment.NewLine)
        rtbInfo.AppendText("  DataDir:   " & AppPaths.DataDir & Environment.NewLine)
        rtbInfo.AppendText("  BackupDir: " & AppPaths.BackupDir & Environment.NewLine)
        rtbInfo.AppendText("  WordDocs:  " & AppPaths.WordDocsDir & Environment.NewLine)

        ' Optional: show a developer image if you have one.
        ' If you already set the image in the designer/resources, you can remove this block.
        Try
            If picDeveloper.Image Is Nothing Then
                ' Example: load from a shared location (edit/remove as needed)
                ' Dim candidate As String = "\\invoice\MainMenu\developer.png"
                ' If IO.File.Exists(candidate) Then picDeveloper.Image = Image.FromFile(candidate)
            End If
        Catch
            ' ignore image load failures
        End Try
    End Sub

End Class