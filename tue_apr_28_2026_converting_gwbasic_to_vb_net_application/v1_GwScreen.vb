Option Strict Off
Option Explicit On

Imports System.Windows.Forms

Public Class GwScreen
    Private ReadOnly rtb As RichTextBox
    Private ReadOnly cols As Integer
    Private ReadOnly rows As Integer

    Private curRow As Integer = 1
    Private curCol As Integer = 1

    Public Sub New(rtb As RichTextBox, cols As Integer, rows As Integer)
        Me.rtb = rtb
        Me.cols = cols
        Me.rows = rows
        EnsureBuffer()
    End Sub

    Public Sub EnsureBuffer()
        Dim lines As New List(Of String)()
        For i = 1 To rows
            lines.Add(New String(" "c, cols))
        Next
        rtb.Text = String.Join(Environment.NewLine, lines)
    End Sub

    Public Sub Cls()
        EnsureBuffer()
        curRow = 1
        curCol = 1
    End Sub

    Public Sub Locate(row As Integer, col As Integer)
        curRow = Math.Max(1, Math.Min(rows, row))
        curCol = Math.Max(1, Math.Min(cols, col))
    End Sub

    Public Sub Color(fg As Integer, bg As Integer)
        ' Simple mapping: ignore for now, or later map to actual colors.
        ' Keeping stub so line-by-line translation compiles.
    End Sub

    Public Sub Print(s As String)
        If s Is Nothing Then s = ""
        Dim allLines = rtb.Lines
        If allLines.Length < rows Then
            EnsureBuffer()
            allLines = rtb.Lines
        End If

        Dim idx = curRow - 1
        Dim line = allLines(idx).PadRight(cols)

        For Each ch In s
            If curCol > cols Then Exit For
            Dim pos = curCol - 1
            line = line.Substring(0, pos) & ch & line.Substring(pos + 1)
            curCol += 1
        Next

        allLines(idx) = line
        rtb.Lines = allLines
    End Sub

    Public Sub PrintLine(Optional s As String = "")
        Print(s)
        curRow = Math.Min(rows, curRow + 1)
        curCol = 1
    End Sub
End Class