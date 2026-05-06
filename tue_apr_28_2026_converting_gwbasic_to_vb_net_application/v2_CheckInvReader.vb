Option Strict On
Option Explicit On

Imports System.Globalization
Imports System.IO

Public NotInheritable Class CheckInvReader
    Private Sub New()
    End Sub

    Public Shared Function ReadAll(path As String) As List(Of CheckInvBlock)
        Dim results As New List(Of CheckInvBlock)()
        If Not File.Exists(path) Then Return results

        Dim rawLines As String() = File.ReadAllLines(path)
        Dim i As Integer = 0

        While i < rawLines.Length
            ' Strip DOS EOF marker (^Z) and whitespace before we do any checks.
            Dim line As String = rawLines(i)
            If line IsNot Nothing Then line = line.Replace(ChrW(26), "")
            line = If(line, "").Trim()

            If line = "" Then
                i += 1
                Continue While
            End If

            ' Header lines start with a quote in your sample.
            If Not line.StartsWith("""", StringComparison.Ordinal) Then
                ' Skip junk/unexpected
                i += 1
                Continue While
            End If

            Dim header = ParseHeader(line)
            i += 1

            Dim invoices As New List(Of String)()
            For n As Integer = 1 To header.InvoiceCount
                If i >= rawLines.Length Then Exit For

                Dim inv As String = rawLines(i)
                If inv IsNot Nothing Then inv = inv.Replace(ChrW(26), "")
                inv = If(inv, "").Trim()

                If inv <> "" Then invoices.Add(inv)
                i += 1
            Next

            header.Invoices = invoices
            results.Add(header)
        End While

        Return results
    End Function

    Private Shared Function ParseHeader(line As String) As CheckInvBlock
        ' Example:
        ' "NATAIRCR","8508","4","01-03-1995",196.75,4
        Dim parts = SplitCsvWriteLine(line)

        ' parts should be: 6 items
        ' 0 customer, 1 checkno, 2 salesman?, 3 date, 4 amount, 5 invoiceCount
        Dim b As New CheckInvBlock() With {
            .CustomerCode = GetPart(parts, 0),
            .CheckNumber = GetPart(parts, 1),
            .SalesmanCode = GetPart(parts, 2),
            .DateText = GetPart(parts, 3),
            .Amount = ParseDecimal(GetPart(parts, 4)),
            .InvoiceCount = ParseInt(GetPart(parts, 5))
        }

        Return b
    End Function

    Private Shared Function GetPart(parts As List(Of String), idx As Integer) As String
        If parts Is Nothing OrElse idx < 0 OrElse idx >= parts.Count Then Return ""
        Return parts(idx)
    End Function

    Private Shared Function ParseDecimal(s As String) As Decimal
        If s Is Nothing Then Return 0D
        s = s.Replace(ChrW(26), "").Trim()

        Dim d As Decimal
        If Decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, d) Then Return d
        If Decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, d) Then Return d
        Return 0D
    End Function

    Private Shared Function ParseInt(s As String) As Integer
        If s Is Nothing Then Return 0
        s = s.Replace(ChrW(26), "").Trim()

        Dim n As Integer
        If Integer.TryParse(s, n) Then Return n
        Return 0
    End Function

    ' Splits a GW-BASIC WRITE# style CSV-ish line:
    ' - quoted strings may contain commas (unlikely here, but we support it)
    ' - numeric fields are unquoted
    Private Shared Function SplitCsvWriteLine(line As String) As List(Of String)
        Dim result As New List(Of String)()
        Dim cur As String = ""
        Dim inQuotes As Boolean = False

        Dim i As Integer = 0
        While i < line.Length
            Dim ch = line(i)

            If ch = """"c Then
                inQuotes = Not inQuotes
                i += 1
                Continue While
            End If

            If ch = ","c AndAlso Not inQuotes Then
                result.Add(cur.Trim())
                cur = ""
                i += 1
                Continue While
            End If

            cur &= ch
            i += 1
        End While

        If cur.Length > 0 OrElse line.EndsWith(",", StringComparison.Ordinal) Then
            result.Add(cur.Trim())
        End If

        Return result
    End Function
End Class

Public Class CheckInvBlock
    Public Property CustomerCode As String
    Public Property CheckNumber As String
    Public Property SalesmanCode As String
    Public Property DateText As String
    Public Property Amount As Decimal
    Public Property InvoiceCount As Integer
    Public Property Invoices As List(Of String)
End Class