Option Strict On
Option Explicit On

Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks

Public Class FormLedgerDoesntBalance
    Inherits System.Windows.Forms.Form

    Private Const ScreenTitle As String = "Checks - Doesn't Balance"
    Private Const Tolerance As Decimal = 1D ' DOS treated < 1 as OK

    Private cts As CancellationTokenSource

    Private ReadOnly dgv As New DataGridView()
    Private ReadOnly btnRefresh As New Button()
    Private ReadOnly lblStatus As New Label()

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        Text = ScreenTitle
        Width = 1200
        Height = 850
        StartPosition = FormStartPosition.CenterParent

        BuildUi()

        ' Fire-and-forget startup load (keeps UI responsive).
        ' Using helper avoids discard-assignment quirks and avoids BC42358 warnings-as-errors.
        FireAndForget(RunReportAsync())
    End Sub

    Private Sub BuildUi()
        Dim top As New FlowLayoutPanel()
        top.Dock = DockStyle.Top
        top.Height = 40
        top.Padding = New Padding(8)
        top.FlowDirection = FlowDirection.LeftToRight
        top.WrapContents = False

        btnRefresh.Text = "Refresh"
        btnRefresh.AutoSize = True
        AddHandler btnRefresh.Click, AddressOf btnRefresh_Click

        lblStatus.AutoSize = True
        lblStatus.Padding = New Padding(12, 8, 0, 0)
        lblStatus.Text = ""

        top.Controls.Add(btnRefresh)
        top.Controls.Add(lblStatus)

        dgv.Dock = DockStyle.Fill
        dgv.ReadOnly = True
        dgv.AllowUserToAddRows = False
        dgv.AllowUserToDeleteRows = False
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgv.AutoGenerateColumns = False

        dgv.Columns.Clear()
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.HeaderText = "Customer", .DataPropertyName = NameOf(BalanceRow.Customer), .Width = 140})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.HeaderText = "Date", .DataPropertyName = NameOf(BalanceRow.DateText), .Width = 110})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.HeaderText = "Check #", .DataPropertyName = NameOf(BalanceRow.CheckNumber), .Width = 110})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.HeaderText = "Check Amount", .DataPropertyName = NameOf(BalanceRow.CheckAmount), .Width = 120})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.HeaderText = "Invoice Count", .DataPropertyName = NameOf(BalanceRow.InvoiceCount), .Width = 110})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.HeaderText = "Invoice Sum", .DataPropertyName = NameOf(BalanceRow.InvoiceSum), .Width = 120})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.HeaderText = "Delta", .DataPropertyName = NameOf(BalanceRow.Delta), .Width = 110})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.HeaderText = "Note", .DataPropertyName = NameOf(BalanceRow.Note), .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})

        Controls.Add(dgv)
        Controls.Add(top)
    End Sub

    Private Async Sub btnRefresh_Click(sender As Object, e As EventArgs)
        Await RunReportAsync()
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        ' Ensure background work is cancelled when closing.
        If cts IsNot Nothing Then
            cts.Cancel()
            cts.Dispose()
            cts = Nothing
        End If

        MyBase.OnFormClosing(e)
    End Sub

    Private Async Function RunReportAsync() As Task
        ' Cancel any prior run (if one is in flight)
        If cts IsNot Nothing Then
            cts.Cancel()
            cts.Dispose()
            cts = Nothing
        End If

        cts = New CancellationTokenSource()
        Dim token As CancellationToken = cts.Token

        btnRefresh.Enabled = False
        lblStatus.Text = "Running..."
        dgv.DataSource = Nothing

        Dim ledgerPath As String = LegacyDataPaths.LedgerCur
        Dim checkInvPath As String = LegacyDataPaths.CheckInv
        Dim invoiceChkPath As String = LegacyDataPaths.InvoiceChk

        If Not IO.File.Exists(ledgerPath) Then
            UiFileErrors.ShowMissingRequiredFile(Me, ScreenTitle, ledgerPath)
            lblStatus.Text = "Missing LEDGER.CUR"
            btnRefresh.Enabled = True
            Return
        End If

        If Not IO.File.Exists(invoiceChkPath) Then
            UiFileErrors.ShowMissingRequiredFile(Me, ScreenTitle, invoiceChkPath)
            lblStatus.Text = "Missing INVOICE.CHK"
            btnRefresh.Enabled = True
            Return
        End If

        Try
            Dim rows As List(Of BalanceRow) =
                Await Task.Run(Function() ComputeRows(ledgerPath, checkInvPath, invoiceChkPath, token), token)

            If token.IsCancellationRequested Then
                lblStatus.Text = "Cancelled"
                Return
            End If

            dgv.DataSource = rows
            lblStatus.Text = $"Rows: {rows.Count:N0}"
        Catch ex As OperationCanceledException
            lblStatus.Text = "Cancelled"
        Catch ex As Exception
            lblStatus.Text = "Error running report"
            MessageBox.Show(Me, ex.ToString(), ScreenTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            btnRefresh.Enabled = True
        End Try
    End Function

    Private Shared Function ComputeRows(
        ledgerPath As String,
        checkInvPath As String,
        invoiceChkPath As String,
        token As CancellationToken
    ) As List(Of BalanceRow)

        token.ThrowIfCancellationRequested()

        ' Load ledger
        Dim ledger As List(Of LedgerEntry) = LedgerCurReader.ReadAll(ledgerPath)

        token.ThrowIfCancellationRequested()

        ' Load CHECK.INV blocks (if missing, treat as no mapping for all)
        Dim invIndex As New Dictionary(Of String, List(Of Long))(StringComparer.OrdinalIgnoreCase)
        If IO.File.Exists(checkInvPath) Then
            Try
                Dim blocks = CheckInvReader.ReadAll(checkInvPath)
                For Each b In blocks
                    token.ThrowIfCancellationRequested()

                    Dim key = BuildKey(b.CustomerCode, b.CheckNumber)
                    Dim lst As List(Of Long) = Nothing
                    If Not invIndex.TryGetValue(key, lst) Then
                        lst = New List(Of Long)()
                        invIndex(key) = lst
                    End If

                    If b.Invoices IsNot Nothing Then
                        For Each s In b.Invoices
                            Dim n As Long
                            If Long.TryParse(If(s, "").Trim(), n) Then
                                lst.Add(n)
                            End If
                        Next
                    End If
                Next
            Catch
                ' Best-effort; keep what we have.
            End Try
        End If

        token.ThrowIfCancellationRequested()

        ' Cache invoice amounts to avoid re-reading INVOICE.CHK for duplicates
        Dim amtCache As New Dictionary(Of Long, Decimal)()

        Dim rows As New List(Of BalanceRow)()

        For Each e In ledger
            token.ThrowIfCancellationRequested()

            Dim key = BuildKey(e.Customer, e.CheckNumber)

            Dim invNos As List(Of Long) = Nothing
            Dim hasMapping As Boolean = invIndex.TryGetValue(key, invNos)

            If Not hasMapping OrElse invNos Is Nothing OrElse invNos.Count = 0 Then
                ' Include "No mapping" rows (your requested scope)
                rows.Add(New BalanceRow With {
                    .Customer = SafeTrimEnd(e.Customer),
                    .DateText = SafeTrimEnd(e.DateText),
                    .CheckNumber = SafeTrim(e.CheckNumber),
                    .CheckAmount = e.Amount.ToString("C"),
                    .InvoiceCount = "0",
                    .InvoiceSum = "",
                    .Delta = "",
                    .Note = "No CHECK.INV mapping"
                })
                Continue For
            End If

            Dim sum As Decimal = 0D
            Dim foundAny As Boolean = False

            For Each invNo In invNos
                token.ThrowIfCancellationRequested()

                Dim a As Decimal = 0D
                If amtCache.TryGetValue(invNo, a) Then
                    sum += a
                    foundAny = True
                Else
                    Dim rec = InvoiceChkReader.ReadRecord(invoiceChkPath, invNo)
                    If rec.IsFound Then
                        a = rec.Amount
                        amtCache(invNo) = a
                        sum += a
                        foundAny = True
                    Else
                        ' Cache missing as 0 so we don't keep re-reading it
                        amtCache(invNo) = 0D
                    End If
                End If
            Next

            Dim delta As Decimal = sum - e.Amount
            Dim unbalanced As Boolean = (Not foundAny) OrElse (Math.Abs(delta) >= Tolerance)

            If unbalanced Then
                Dim note As String = If(Not foundAny, "Invoices not found in INVOICE.CHK", "")
                rows.Add(New BalanceRow With {
                    .Customer = SafeTrimEnd(e.Customer),
                    .DateText = SafeTrimEnd(e.DateText),
                    .CheckNumber = SafeTrim(e.CheckNumber),
                    .CheckAmount = e.Amount.ToString("C"),
                    .InvoiceCount = invNos.Count.ToString(),
                    .InvoiceSum = sum.ToString("C"),
                    .Delta = delta.ToString("C"),
                    .Note = note
                })
            End If
        Next

        Return rows
    End Function

    Private Shared Sub FireAndForget(t As Task)
        ' Intentionally ignore the task. Observe exceptions to avoid UnobservedTaskException.
        If t Is Nothing Then Return

        t.ContinueWith(
            Sub(x) Dim _ = x.Exception,
            TaskContinuationOptions.OnlyOnFaulted
        )
    End Sub

    Private Shared Function BuildKey(code As String, checkNum As String) As String
        Return If(code, "").TrimEnd() & "|" & If(checkNum, "").Trim()
    End Function

    Private Shared Function SafeTrim(s As String) As String
        Return If(s, "").Trim()
    End Function

    Private Shared Function SafeTrimEnd(s As String) As String
        Return If(s, "").TrimEnd()
    End Function

    Private NotInheritable Class BalanceRow
        Public Property Customer As String
        Public Property DateText As String
        Public Property CheckNumber As String
        Public Property CheckAmount As String
        Public Property InvoiceCount As String
        Public Property InvoiceSum As String
        Public Property Delta As String
        Public Property Note As String
    End Class
End Class