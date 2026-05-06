Option Strict On
Option Explicit On

Imports System.Globalization
Imports System.IO
Imports Microsoft.VisualBasic.FileIO

Public NotInheritable Class LedgerCurReader
    Private Sub New()
    End Sub

    Public Shared Function ReadAll(path As String) As List(Of LedgerEntry)
        Dim results As New List(Of LedgerEntry)()

        If Not File.Exists(path) Then
            Return results
        End If

        ' GW-BASIC wrote 6 logical fields per entry, but they may be split across lines.
        ' We'll read *fields* continuously and group by 6.
        Dim buffer As New List(Of String)()

        Using p As New TextFieldParser(path)
            p.TextFieldType = FieldType.Delimited
            p.SetDelimiters(",")
            p.HasFieldsEnclosedInQuotes = True
            p.TrimWhiteSpace = True

            While Not p.EndOfData
                Dim fields = p.ReadFields()
                If fields Is Nothing Then Continue While

                For Each f In fields
                    buffer.Add(If(f, ""))
                    If buffer.Count = 6 Then
                        results.Add(ParseEntry(buffer))
                        buffer.Clear()
                    End If
                Next
            End While
        End Using

        ' If buffer leftover exists, file is malformed; ignore tail.
        Return results
    End Function

    Private Shared Function ParseEntry(fields As List(Of String)) As LedgerEntry
        Dim e As New LedgerEntry()

        e.Customer = fields(0).Trim()
        e.DateText = fields(1).Trim()
        e.CheckNumber = fields(2).Trim()
        e.InvoiceDiffText = fields(3).Trim()
        e.Amount = ParseDecimal(fields(4))
        e.Reference = fields(5).Trim()

        Return e
    End Function

    Private Shared Function ParseDecimal(s As String) As Decimal
        s = (If(s, "")).Trim()

        Dim d As Decimal
        If Decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, d) Then Return d
        If Decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, d) Then Return d

        Return 0D
    End Function
End Class

Public Class LedgerEntry
    Public Property Customer As String
    Public Property DateText As String
    Public Property CheckNumber As String
    Public Property InvoiceDiffText As String
    Public Property Amount As Decimal
    Public Property Reference As String
End Class