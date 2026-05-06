Option Strict On
Option Explicit On

Public Class FormLedgerView

    Private _all As List(Of LedgerEntry) = New List(Of LedgerEntry)()

    Private Sub FormLedgerView_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Ledger - View Checks"

        cmbMonth.Items.Clear()
        cmbMonth.Items.Add("") ' all
        For m As Integer = 1 To 12
            cmbMonth.Items.Add(m.ToString("00"))
        Next

        cmbYear.Items.Clear()
        cmbYear.Items.Add("") ' all
        For y As Integer = Date.Today.Year To 1986 Step -1
            cmbYear.Items.Add(y.ToString())
        Next

        LoadData()
        ApplyFilters()
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        LoadData()
        ApplyFilters()
    End Sub

    Private Sub txtCustomer_TextChanged(sender As Object, e As EventArgs) Handles txtCustomer.TextChanged
        ApplyFilters()
    End Sub

    Private Sub txtCheck_TextChanged(sender As Object, e As EventArgs) Handles txtCheck.TextChanged
        ApplyFilters()
    End Sub

    Private Sub cmbYear_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbYear.SelectedIndexChanged
        ApplyFilters()
    End Sub

    Private Sub cmbMonth_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbMonth.SelectedIndexChanged
        ApplyFilters()
    End Sub

    Private Sub LoadData()
        _all = LedgerCurReader.ReadAll(LegacyDataPaths.LedgerCur)

        dgv.AutoGenerateColumns = True
        dgv.DataSource = _all
    End Sub

    Private Sub ApplyFilters()
        Dim cust As String = txtCustomer.Text.Trim()
        Dim chk As String = txtCheck.Text.Trim()
        Dim year As String = If(TryCast(cmbYear.SelectedItem, String), "").Trim()
        Dim month As String = If(TryCast(cmbMonth.SelectedItem, String), "").Trim()

        Dim filtered = _all.AsEnumerable()

        If cust <> "" Then
            ' DOS used LEFT$(cust,8) exact-ish; we’ll do “contains” but you can switch to StartsWith.
            filtered = filtered.Where(Function(e) e.Customer IsNot Nothing AndAlso e.Customer.IndexOf(cust, StringComparison.OrdinalIgnoreCase) >= 0)
        End If

        If chk <> "" Then
            filtered = filtered.Where(Function(e) e.CheckNumber IsNot Nothing AndAlso e.CheckNumber.IndexOf(chk, StringComparison.OrdinalIgnoreCase) >= 0)
        End If

        If year <> "" Then
            ' GW-BASIC checked RIGHT$(DT$,2) vs RIGHT$(YEAR$,2). DT$ is MM-DD-YYYY.
            Dim yy As String = year.Substring(year.Length - 2)
            filtered = filtered.Where(Function(e) e.DateText IsNot Nothing AndAlso e.DateText.Length >= 2 AndAlso e.DateText.EndsWith(yy, StringComparison.OrdinalIgnoreCase))
        End If

        If month <> "" Then
            filtered = filtered.Where(Function(e) e.DateText IsNot Nothing AndAlso e.DateText.Length >= 2 AndAlso e.DateText.StartsWith(month, StringComparison.OrdinalIgnoreCase))
        End If

        Dim list = filtered.ToList()
        dgv.DataSource = list

        Dim total As Decimal = list.Sum(Function(e) e.Amount)
        lblTotal.Text = $"Total: {total:C}"
    End Sub

End Class