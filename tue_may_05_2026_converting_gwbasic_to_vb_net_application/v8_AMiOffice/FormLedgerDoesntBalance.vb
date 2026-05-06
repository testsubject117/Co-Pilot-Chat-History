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
    Private ReadOnly btnCancel As New Button()
    Private ReadOnly cboFilter As New ComboBox()
    Private ReadOnly lblStatus As New Label()

    ' Keep the full results so we can filter/sort without re-running the report.
    Private allRows As List(Of BalanceRow) = New List(Of BalanceRow)()

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        Text = ScreenTitle
        Width = 1200
        Height = 850
        StartPosition = FormStartPosition.CenterParent

        BuildUi()

        ' Fire-and-forget startup load (keeps UI responsive)
        FireAndForget(RunReportAsync())
    End Sub

    Private Sub BuildUi()
        Dim top As New FlowLayoutPanel()
        top.Dock = DockStyle.Top
        top.Height = 44
        top.Padding = New Padding(8)
        top.FlowDirection = FlowDirection.LeftToRight
        top.WrapContents = False

        btnRefresh.Text = "Refresh"
        btnRefresh.AutoSize = True
        AddHandler btnRefresh.Click, AddressOf btnRefresh_Click

        btnCancel.Text = "Cancel"
        btnCancel.AutoSize = True
        btnCancel.Enabled = False
        AddHandler btnCancel.Click, AddressOf btnCancel_Click

        cboFilter.DropDownStyle = ComboBoxStyle.DropDownList
        cboFilter.Width = 220
        cboFilter.Items.Clear()
        cboFilter.Items.Add(FilterMode.AllUnbalanced)
        cboFilter.Items.Add(FilterMode.NoMapping)
        cboFilter.Items.Add(FilterMode.DeltaOnly)
        cboFilter.SelectedIndex = 0
        AddHandler cboFilter.SelectedIndexChanged, AddressOf cboFilter_SelectedIndexChanged

        lblStatus.AutoSize = True
        lblStatus.Padding = New Padding(12, 10, 0, 0)
        lblStatus.Text = ""

        top.Controls.Add(btnRefresh)
        top.Controls.Add(btnCancel)
        top.Controls.Add(New Label() With {.AutoSize = True, .Padding = New Padding(8, 10, 0, 0), .Text = "Filter:"})
        top.Controls.Add(cboFilter)
        top.Controls.Add(lblStatus)

        dgv.Dock = DockStyle.Fill
        dgv.ReadOnly = True
        dgv.AllowUserToAddRows = False
        dgv.AllowUserToDeleteRows = False
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgv.AutoGenerateColumns = False
        dgv.MultiSelect = False

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

    Private Sub btnCancel_Click(sender As Object, e As EventArgs)
        If cts IsNot Nothing Then
            cts.Cancel()
        End If
    End Sub

    Private Async Sub btnRefresh_Click(sender As Object, e As EventArgs)
        Await RunReportAsync()
    End Sub

    Private Sub cboFilter_SelectedIndexChanged(sender As Object, e As EventArgs)
        ApplyFilterAndBind()
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
        btnCancel.Enabled = True
        cboFilter.Enabled = False
        lblStatus.Text = "Starting..."
        dgv.DataSource = Nothing
        allRows = New List(Of BalanceRow)()

        Dim ledgerPath As String = LegacyDataPaths.LedgerCur
        Dim checkInvPath As String = LegacyDataPaths.CheckInv
        Dim invoiceChkPath As String = LegacyDataPaths.InvoiceChk

        If Not IO.File.Exists(ledgerPath) Then
            UiFileErrors.ShowMissingRequiredFile(Me, ScreenTitle, ledgerPath)
            lblStatus.Text = "Missing LEDGER.CUR"
            btnRefresh.Enabled = True
            btnCancel.Enabled = False
            cboFilter.Enabled = True
            Return
        End If

        ' Progress: safely update UI from background thread.
        Dim progress As IProgress(Of ProgressInfo) =
            New Progress(Of ProgressInfo)(
                Sub(p As ProgressInfo)
                    If token.IsCancellationRequested Then Return
                    If p.Total > 0 Then
                        lblStatus.Text = $"{p.Message}  ({p.Current:N0}/{p.Total:N0})"
                    Else
                        lblStatus.Text = p.Message
                    End If
                End Sub
            )

        Try
            Dim rows As List(Of BalanceRow) =
                Await Task.Run(Function() ComputeRows(ledgerPath, checkInvPath, invoiceChkPath, token, progress), token)

            If token.IsCancellationRequested Then
                lblStatus.Text = "Cancelled"
                Return
            End If

            allRows = rows
            ApplyFilterAndBind()

            lblStatus.Text = $"Rows: {dgv.RowCount:N0} (filter: {TryCast(cboFilter.SelectedItem, String)})"
        Catch ex As OperationCanceledException
            lblStatus.Text = "Cancelled"
        Catch ex As Exception
            lblStatus.Text = "Error running report"
            MessageBox.Show(Me, ex.ToString(), ScreenTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            btnRefresh.Enabled = True
            btnCancel.Enabled = False
            cboFilter.Enabled = True
        End Try
    End Function

    Private Sub ApplyFilterAndBind()
        Dim mode As String = TryCast(cboFilter.SelectedItem, String)
        If String.IsNullOrWhiteSpace(mode) Then mode = FilterMode.AllUnbalanced

        Dim filtered As IEnumerable(Of BalanceRow) = allRows

        Select Case mode
            Case FilterMode.NoMapping
                filtered = filtered.Where(Function(r) r.IsNoMapping)
            Case FilterMode.DeltaOnly
                filtered = filtered.Where(Function(r) Not r.IsNoMapping)
            Case Else
                ' AllUnbalanced (no extra filter)
        End Select

        ' Sort: biggest absolute delta first; keep "No mapping" at top by treating as huge delta.
        Dim sorted As List(Of BalanceRow) =
            filtered.OrderByDescending(Function(r) If(r.IsNoMapping, Decimal.MaxValue, Math.Abs(r.DeltaValue))).ToList()

        dgv.DataSource = sorted
    End Sub

    Private Shared Function ComputeRows(
        ledgerPath As String,
        checkInvPath As String,
        invoiceChkPath As String,
        token As CancellationToken,
        progress As IProgress(Of ProgressInfo)
    ) As List(Of BalanceRow)

        token.ThrowIfCancellationRequested()
        progress?.Report(New ProgressInfo("Loading ledger...", 0, 0))

        ' Load ledger
        Dim ledger As List(Of LedgerEntry) = LedgerCurReader.ReadAll(ledgerPath)
        token.ThrowIfCancellationRequested()

        progress?.Report(New ProgressInfo("Loading CHECK.INV...", 0, 0))

        ' Load CHECK.INV blocks (if missing, treat as no mapping for all)
        Dim invIndex As New Dictionary(Of String, List(Of Long))(StringComparer.OrdinalIgnoreCase)
        If IO.File.Exists(checkInvPath) Then
            Try
                Dim blocks = CheckInvReader.ReadAll(checkInvPath)
                Dim i As Integer = 0
                For Each b In blocks
                    token.ThrowIfCancellationRequested()
                    i += 1
                    If (i Mod 250) = 0 Then
                        progress?.Report(New ProgressInfo("Indexing CHECK.INV...", i, blocks.Count))
                    End If

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

        progress?.Report(New ProgressInfo("Computing rows...", 0, ledger.Count))

        ' Cache invoice amounts to avoid re-reading INVOICE.CHK for duplicates
        Dim amtCache As New Dictionary(Of Long, Decimal)()

        Dim rows As New List(Of BalanceRow)()

        Dim idx As Integer = 0
        For Each e In ledger
            token.ThrowIfCancellationRequested()

            idx += 1
            If (idx Mod 250) = 0 Then
                progress?.Report(New ProgressInfo("Computing rows...", idx, ledger.Count))
            End If

            Dim key = BuildKey(e.Customer, e.CheckNumber)

            Dim invNos As List(Of Long) = Nothing
            Dim hasMapping As Boolean = invIndex.TryGetValue(key, invNos)

            If Not hasMapping OrElse invNos Is Nothing OrElse invNos.Count = 0 Then
                rows.Add(New BalanceRow With {
                    .Customer = SafeTrimEnd(e.Customer),
                    .DateText = SafeTrimEnd(e.DateText),
                    .CheckNumber = SafeTrim(e.CheckNumber),
                    .CheckAmountValue = e.Amount,
                    .InvoiceCountValue = 0,
                    .InvoiceSumValue = 0D,
                    .DeltaValue = 0D,
                    .Note = "No CHECK.INV mapping",
                    .IsNoMapping = True
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
                    .CheckAmountValue = e.Amount,
                    .InvoiceCountValue = invNos.Count,
                    .InvoiceSumValue = sum,
                    .DeltaValue = delta,
                    .Note = note,
                    .IsNoMapping = False
                })
            End If
        Next

        progress?.Report(New ProgressInfo("Done.", ledger.Count, ledger.Count))
        Return rows
    End Function

    Private Shared Sub FireAndForget(t As Task)
        ' Intentionally ignore the task. Observe exceptions to avoid UnobservedTaskException.
        If t Is Nothing Then Return

        t.ContinueWith(
            Sub(completed As Task)
                Dim ignored = completed.Exception
            End Sub,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default
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

    Private NotInheritable Class FilterMode
        Public Const AllUnbalanced As String = "All unbalanced"
        Public Const NoMapping As String = "No CHECK.INV mapping"
        Public Const DeltaOnly As String = "Delta >= $1 (has mapping)"
    End Class

    Private NotInheritable Class ProgressInfo
        Public ReadOnly Property Message As String
        Public ReadOnly Property Current As Integer
        Public ReadOnly Property Total As Integer

        Public Sub New(message As String, current As Integer, total As Integer)
            Me.Message = message
            Me.Current = current
            Me.Total = total
        End Sub
    End Class

    Private Class BalanceRow
        Public Property Customer As String
        Public Property DateText As String
        Public Property CheckNumber As String

        ' Backing numeric values (for sorting/filtering and consistent formatting)
        Public Property CheckAmountValue As Decimal
        Public Property InvoiceCountValue As Integer
        Public Property InvoiceSumValue As Decimal
        Public Property DeltaValue As Decimal

        Public Property Note As String
        Public Property IsNoMapping As Boolean

        ' Display properties bound to the grid
        Public ReadOnly Property CheckAmount As String
            Get
                Return CheckAmountValue.ToString("C")
            End Get
        End Property

        Public ReadOnly Property InvoiceCount As String
            Get
                Return InvoiceCountValue.ToString()
            End Get
        End Property

        Public ReadOnly Property InvoiceSum As String
            Get
                Return If(IsNoMapping AndAlso InvoiceCountValue = 0, "", InvoiceSumValue.ToString("C"))
            End Get
        End Property

        Public ReadOnly Property Delta As String
            Get
                Return If(IsNoMapping, "", DeltaValue.ToString("C"))
            End Get
        End Property
    End Class
End Class