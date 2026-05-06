Option Strict On
Option Explicit On

Imports System.Diagnostics
Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Text

Partial Public Class FormAbout

    Private Sub FormAbout_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Window / header labels
        Me.Text = "About"
        lblAbout.Text = "About:"
        lblAppName.Text = "Active Magnetic Inspection Main Menu Application"
        lblDevelopedBy.Text = "Developed By:"

        ' Restore the ESC logo
        picDeveloper.Image = LoadEmbeddedImage({"ESC Logo.jpg", "AMiOffice.ESC Logo.jpg", "AMiOffice.ESC_Logo.jpg"})
        picDeveloper.Visible = (picDeveloper.Image IsNot Nothing)

        ' Build the info block (RichTextBox)
        rtbInfo.Clear()
        rtbInfo.SelectionAlignment = HorizontalAlignment.Center

        Dim exePath As String = Application.ExecutablePath
        Dim fvi As FileVersionInfo = FileVersionInfo.GetVersionInfo(exePath)

        Dim sb As New StringBuilder()

        ' App identity
        sb.AppendLine("AMiOffice")
        sb.AppendLine($"Version: {My.Application.Info.Version}")
        sb.AppendLine($"File Version: {Safe(fvi.FileVersion)}")
        sb.AppendLine($"Build: {BuildInfo.DisplayVersion}")
        sb.AppendLine()

        ' Author/company info (from your screenshot)
        sb.AppendLine("Author/Developer: Kirk Saffell")
        sb.AppendLine("Company: Enterprise Services & Consulting")
        sb.AppendLine("Phone: (661) 478-0990")
        sb.AppendLine("Contact: capnkirk@capnkirk.com")
        sb.AppendLine("Year: 2026")
        sb.AppendLine()

        ' Machine info
        sb.AppendLine($"Machine: {Environment.MachineName}")
        sb.AppendLine($"User: {Environment.UserName}")
        sb.AppendLine($"OS: {RuntimeInformation.OSDescription}")
        sb.AppendLine($".NET Runtime: {RuntimeInformation.FrameworkDescription}")
        sb.AppendLine()

        ' Shared paths (adjust these strings if needed)
        sb.AppendLine("Shared paths:")

        Dim dataPath As String = "\\invoice\MainMenu\Data"
        Dim backupsPath As String = "\\invoice\MainMenu\Backups"
        Dim wordPath As String = "\\invoice\word"

        sb.AppendLine($"Data: {dataPath} {ExistsTag(dataPath)}")
        sb.AppendLine($"Backups: {backupsPath} {ExistsTag(backupsPath)}")
        sb.AppendLine($"Word: {wordPath} {ExistsTag(wordPath)}")

        rtbInfo.Text = sb.ToString()
    End Sub

    Private Shared Function ExistsTag(path As String) As String
        Try
            If Directory.Exists(path) Then
                Return ""
            End If
        Catch
            ' ignore (permissions/network issues) and treat as not found
        End Try

        Return "[NOT FOUND]"
    End Function

    Private Shared Function Safe(value As String, Optional fallback As String = "") As String
        If String.IsNullOrWhiteSpace(value) Then
            If String.IsNullOrWhiteSpace(fallback) Then Return "(not set)"
            Return fallback
        End If
        Return value.Trim()
    End Function

    Private Shared Function LoadEmbeddedImage(candidateNames As IEnumerable(Of String)) As Image
        Dim asm As Assembly = Assembly.GetExecutingAssembly()
        Dim allNames As String() = asm.GetManifestResourceNames()

        For Each candidate In candidateNames
            ' Try exact match first
            Dim exact As String = allNames.FirstOrDefault(Function(n) String.Equals(n, candidate, StringComparison.OrdinalIgnoreCase))
            If exact Is Nothing Then
                ' Try "ends with" match (common with VB root namespace prefixes)
                exact = allNames.FirstOrDefault(Function(n) n.EndsWith(candidate, StringComparison.OrdinalIgnoreCase))
            End If

            If exact IsNot Nothing Then
                Using s As Stream = asm.GetManifestResourceStream(exact)
                    If s IsNot Nothing Then
                        Return Image.FromStream(s)
                    End If
                End Using
            End If
        Next

        Return Nothing
    End Function

End Class