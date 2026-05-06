Option Strict Off
Option Explicit On

Imports System.Linq
Imports System.Diagnostics
Imports System.Reflection
Imports System.IO
Imports System.Runtime.InteropServices

Public Class FormAbout

    Private Const AppTitle As String =
    "About:" & vbCrLf & """Active Magnetic Inspection Main Menu Application"""

    Private Const AuthorName As String = "Kirk Saffell"
    Private Const DeveloperCompany As String = "Enterprise Services & Consulting"
    Private Const ContactEmail As String = "capnkirk@capnkirk.com"
    Private Const AuthorPhone As String = "(661) 478-0990"
    Private Const CopyrightYear As String = "2026"

    Private Sub FormAbout_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Title handled by designer labels now
        LoadDeveloperImage()

        Dim aboutAsm As Assembly = Assembly.GetExecutingAssembly()

        Dim aboutProduct As String = Application.ProductName

        Dim aboutVer As String = ""
        Try
            aboutVer = aboutAsm.GetName().Version?.ToString()
        Catch
            aboutVer = ""
        End Try

        ' File Version (best pulled from the built EXE metadata)
        Dim aboutFileVer As String = ""
        Try
            Dim fvi As FileVersionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath)
            aboutFileVer = fvi.FileVersion
        Catch
            aboutFileVer = ""
        End Try

        ' Informational Version (optional; often blank in .NET Framework unless set)
        Dim aboutInfoVer As String = ""
        Try
            aboutInfoVer = aboutAsm.GetCustomAttribute(Of AssemblyInformationalVersionAttribute)()?.InformationalVersion
        Catch
            aboutInfoVer = ""
        End Try

        ' Build date based on EXE last write time (updates each rebuild/publish)
        Dim aboutBuildDate As String = "unknown"
        Try
            aboutBuildDate = File.GetLastWriteTime(aboutAsm.Location).ToString("yyyy-MM-dd HH:mm:ss")
        Catch
            aboutBuildDate = "unknown"
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
            "",
            "",
            $"{aboutProduct}",
            $"Version: {If(String.IsNullOrWhiteSpace(aboutVer), "(unknown)", aboutVer)}",
            $"File Version: {If(String.IsNullOrWhiteSpace(aboutFileVer), "(not set)", aboutFileVer)}",
            $"Build: {If(String.IsNullOrWhiteSpace(aboutInfoVer), "(not set)", aboutInfoVer)}",
            $"Build date: {aboutBuildDate}",
            "",
            $"Author/Developer: {AuthorName}",
            $"Company: {DeveloperCompany}",
            $"Phone: {AuthorPhone}",
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

        rtbInfo.Clear()
        rtbInfo.Text = String.Join(Environment.NewLine, lines)

        rtbInfo.SelectAll()
        rtbInfo.SelectionAlignment = HorizontalAlignment.Center
        rtbInfo.DeselectAll()

        rtbInfo.SelectionStart = 0
        rtbInfo.SelectionLength = 0
    End Sub

    Private Sub LoadDeveloperImage()
        Try
            If picDeveloper Is Nothing Then Return

            Dim imgAsm As Assembly = Assembly.GetExecutingAssembly()
            Dim names = imgAsm.GetManifestResourceNames()

            Dim resName As String = Nothing

            ' Most common case: spaces become underscores in the manifest name
            resName = names.FirstOrDefault(Function(n) n.EndsWith(".ESC_Logo.jpg", StringComparison.OrdinalIgnoreCase))
            If String.IsNullOrEmpty(resName) Then
                resName = names.FirstOrDefault(Function(n) n.EndsWith(".ESC_Logo.png", StringComparison.OrdinalIgnoreCase))
            End If

            ' Fallbacks
            If String.IsNullOrEmpty(resName) Then
                resName = names.FirstOrDefault(Function(n) n.IndexOf("esc", StringComparison.OrdinalIgnoreCase) >= 0 AndAlso
                                                         (n.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) OrElse
                                                          n.EndsWith(".png", StringComparison.OrdinalIgnoreCase)))
            End If

            If String.IsNullOrEmpty(resName) Then
                picDeveloper.Visible = False
                Return
            End If

            Using s = imgAsm.GetManifestResourceStream(resName)
                If s Is Nothing Then
                    picDeveloper.Visible = False
                    Return
                End If

                Using ms As New MemoryStream()
                    s.CopyTo(ms)
                    ms.Position = 0
                    picDeveloper.Image = Image.FromStream(ms)
                End Using
            End Using

            picDeveloper.SizeMode = PictureBoxSizeMode.Zoom
            picDeveloper.Visible = True

        Catch ex As Exception
#If DEBUG Then
            MessageBox.Show(ex.ToString(), "LoadDeveloperImage error")
#End If
            If picDeveloper IsNot Nothing Then picDeveloper.Visible = False
        End Try
    End Sub

End Class