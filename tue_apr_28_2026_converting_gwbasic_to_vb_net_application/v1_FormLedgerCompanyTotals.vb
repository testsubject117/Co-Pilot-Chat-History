Option Strict On
Option Explicit On

Imports System.Linq

Public Class FormLedgerCompanyTotals

    Private ReadOnly dgv As New DataGridView()
    Private ReadOnly btnClose As New Button()

    Public Sub New()
        Text = "Ledger - Company Totals"
        Width = 800
        Height = 700
        StartPosition = FormStartPosition.CenterParent

        dgv.Dock = DockStyle.Fill
        dgv.ReadOnly = True
        dgv.AllowUserToAddRows = False
        dgv.AllowUserToDeleteRows = False
        dgv.AutoGenerateColumns = True
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect

        btnClose.Text = "Close"
        btnClose.Dock = DockStyle.Bottom
        btnClose.Height = 40
        AddHandler btnClose.Click, Sub() Close()

        Controls.Add(dgv)
        Controls.Add(btnClose)
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)

        Try
            Dim entries = LedgerCurReader.ReadAll(LegacyDataPaths.LedgerCur)

            Dim totals =
                entries.
                GroupBy(Function(x) (If(x.Customer, "")).Trim()).
                Select(Function(g) New With {
                    .Customer = g.Key,
                    .Total = g.Sum(Function(x) x.Amount),
                    .Count = g.Count()
                }).
                OrderBy(Function(x) x.Customer).
                ToList()

            dgv.DataSource = totals
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, "Company Totals", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class