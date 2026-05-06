Option Strict On
Option Explicit On

Imports System.Diagnostics
Imports System.Reflection
Imports System.Windows.Forms

Public Class FormAbout

    Private Sub FormAbout_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Always show a stable app title (avoid "My")
        lblTitle.Text = "AMiOffice"

        ' Your custom build display (e.g. "AMiOffice v1.0 (Build 14)")
        lblDisplayVersion.Text = SafeText(BuildInfo.DisplayVersion, "AMiOffice")

        ' Assembly version (from VB Application Framework)
        lblAssemblyVersion.Text = My.Application.Info.Version.ToString()

        ' File/Product version come from the built EXE metadata
        Dim exePath As String = Application.ExecutablePath
        Dim fvi As FileVersionInfo = FileVersionInfo.GetVersionInfo(exePath)

        ' If you have these labels, populate them; otherwise remove these lines.
        If ControlExists("lblFileVersion") Then
            DirectCast(Controls.Find("lblFileVersion", True)(0), Label).Text =
                SafeText(fvi.FileVersion, My.Application.Info.Version.ToString())
        End If

        If ControlExists("lblProductVersion") Then
            DirectCast(Controls.Find("lblProductVersion", True)(0), Label).Text =
                SafeText(fvi.ProductVersion, BuildInfo.ProductVersion)
        End If

        If ControlExists("lblProductName") Then
            DirectCast(Controls.Find("lblProductName", True)(0), Label).Text =
                SafeText(fvi.ProductName, "AMiOffice")
        End If

        If ControlExists("lblCompanyName") Then
            DirectCast(Controls.Find("lblCompanyName", True)(0), Label).Text =
                SafeText(fvi.CompanyName, "")
        End If

        If ControlExists("lblCopyright") Then
            DirectCast(Controls.Find("lblCopyright", True)(0), Label).Text =
                SafeText(fvi.LegalCopyright, "")
        End If
    End Sub

    Private Shared Function SafeText(value As String, fallback As String) As String
        If String.IsNullOrWhiteSpace(value) Then
            Return fallback
        End If
        Return value
    End Function

    Private Function ControlExists(controlName As String) As Boolean
        Dim matches = Me.Controls.Find(controlName, True)
        Return matches IsNot Nothing AndAlso matches.Length > 0
    End Function

End Class