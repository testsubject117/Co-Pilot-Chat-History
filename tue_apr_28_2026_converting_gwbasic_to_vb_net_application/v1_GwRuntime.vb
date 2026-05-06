Option Strict Off
Option Explicit On

Imports System.IO
Imports System.Collections.Generic
Imports System.Windows.Forms

Public Module GwRuntime
    Private keyQueue As New Queue(Of String)()

    Public Sub Reset()
        keyQueue.Clear()
    End Sub

    Public Sub PushKey(ch As String)
        If ch Is Nothing Then ch = ""
        If ch.Length > 1 Then ch = ch.Substring(0, 1)
        keyQueue.Enqueue(ch)
    End Sub

    Public Function InKey() As String
        If keyQueue.Count = 0 Then Return ""
        Return keyQueue.Dequeue()
    End Function

    Public Function ReadFirstLine(path As String) As String
        If Not File.Exists(path) Then Throw New FileNotFoundException(path)
        Using sr As New StreamReader(path)
            Return sr.ReadLine()
        End Using
    End Function

    Public Sub WriteLine(path As String, value As String)
        Using sw As New StreamWriter(path, append:=False)
            sw.WriteLine(value)
        End Using
    End Sub

    Public Sub NotImplemented(feature As String)
        MessageBox.Show("Not implemented yet: " & feature, "Port status", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Public Function ChrW(code As Integer) As String
        Return Microsoft.VisualBasic.ChrW(code)
    End Function

    Public Function KeyEventToInkeyChar(e As KeyEventArgs) As String
        ' Approximate INKEY$ for single keys (letters/numbers/plus)
        If e.KeyCode >= Keys.A AndAlso e.KeyCode <= Keys.Z Then
            Dim ch As Char = ChrW(AscW("A"c) + (e.KeyCode - Keys.A))
            If e.Shift Then Return ch
            Return Char.ToLowerInvariant(ch)
        End If

        If e.KeyCode >= Keys.D0 AndAlso e.KeyCode <= Keys.D9 Then
            Dim ch As Char = ChrW(AscW("0"c) + (e.KeyCode - Keys.D0))
            Return ch
        End If

        If e.KeyCode = Keys.Add OrElse e.KeyCode = Keys.Oemplus Then Return "+"
        If e.KeyCode = Keys.Oem5 Then Return "%" ' best-effort; keyboard dependent
        If e.KeyCode = Keys.Oem4 Then Return "$" ' best-effort

        Return ""
    End Function
End Module