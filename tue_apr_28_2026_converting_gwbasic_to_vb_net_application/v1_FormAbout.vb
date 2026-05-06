Option Strict Off
Option Explicit On

Imports System.Reflection
Imports System.IO
Imports System.Runtime.InteropServices

Public Class FormAbout

    ' TODO: customize these
    Private Const AuthorName As String = "YOUR NAME"
    Private Const ContactEmail As String = "you@example.com"
    Private Const CopyrightYear As String = "2026"

    Private Sub FormAbout_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim asm = Assembly.GetExecutingAssembly()

        Dim product As String = Application.ProductName
        Dim ver As String = asm.GetName().Version?.ToString()

        ' "Build number" options:
        ' - File version (good if you set it in Assembly Information)
        ' - Informational version (good if you set it manually)
        Dim fileVer As String = ""
        Try
            fileVer = asm.GetCustomAttribute(Of AssemblyFileVersionAttribute)()?.Version
        Catch
        End Try

        Dim infoVer As String = ""
        Try
            infoVer = asm.GetCustomAttribute(Of AssemblyInformationalVersionAttribute)()?.InformationalVersion
        Catch
        End Try

        ' Build date: last write time of the EXE on disk (simple + accurate for deployed build)
        Dim buildDate As String = "unknown"
        Try
            buildDate = File.GetLastWriteTime(asm.Location).ToString("yyyy-MM-dd HH:mm:ss")
        Catch
        End Try

        Dim shareOk As Func(Of String, String) =
            Function(p As String)
                Try
                    Return If(Directory.Exists(p), "OK", "NOT FOUND")
                Catch
                    Return "ERROR"
                End Try
            End Function

        Dim lines As New List(Of String) From {
            $"{product}",
            $"Version: {If(String.IsNullOrWhiteSpace(ver), "(unknown)", ver)}",
            $"File Version: {If(String.IsNullOrWhiteSpace(fileVer), "(not set)", fileVer)}",
            $"Build: {If(String.IsNullOrWhiteSpace(infoVer), "(not set)", infoVer)}",
            $"Build Date (file time): {buildDate}",
            "",
            $"Author: {AuthorName}",
            $"Contact: {ContactEmail}",
            $"Year: {CopyrightYear}",
            "",
            $"Machine: {Environment.MachineName}",
            $"User: {Environment.UserName}",
            $"OS: {Environment.OSVersion}",
            $"Runtime: {RuntimeInformation.FrameworkDescription}",
            "",
            "Shared paths:",
            $"  Data:    {AppPaths.DataDir}  [{shareOk(AppPaths.DataDir)}]",
            $"  Backups: {AppPaths.BackupDir}  [{shareOk(AppPaths.BackupDir)}]",
            $"  Word:    {AppPaths.WordDocsDir}  [{shareOk(AppPaths.WordDocsDir)}]"
        }

        lblInfo.Text = String.Join(Environment.NewLine, lines)
    End Sub

End Class