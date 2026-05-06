Option Strict On
Option Explicit On

Imports System.Linq

Public Class FormFindByCheckNumber

    Private ReadOnly txtCheck As New TextBox()
    Private ReadOnly btnFind As New Button()
    Private ReadOnly lblHeader As New Label()
    Private ReadOnly lstInvoices As New ListBox()

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        Text = "Find by Check # (CHECK.INV)"
        Width = 700
        Height = 650
        StartPosition = FormStartPosition.CenterParent

        Dim top As New FlowLayoutPanel() With {.Dock = DockStyle.Top, .Height = 45, .Padding = New Padding(10)}
        top.Controls.Add(New Label() With {.Text = "Check #:", .AutoSize = True, .Padding = New Padding(0, 10, 0, 0)})
        txtCheck.Width = 150
        top.Controls.Add(txtCheck)
        btnFind.Text = "Find"
        top.Controls.Add(btnFind)

        lblHeader.Dock = DockStyle.Top
        lblHeader.Height = 50
        lblHeader.Padding = New Padding(10)
        lblHeader.Text = "Enter a check # and click Find."

        lstInvoices.Dock = DockStyle.Fill

        Controls.Add(lstInvoices)
        Controls.Add(lblHeader)
        Controls.Add(top)

        AddHandler btnFind.Click, Sub() DoFind()
        AddHandler txtCheck.KeyDown,
            Sub(sender, args)
                If args.KeyCode = Keys.Enter Then
                    args.SuppressKeyPress = True
                    DoFind()
                End If
            End Sub
    End Sub

    Private Sub DoFind()
        Dim checkNo = txtCheck.Text.Trim()
        If checkNo = "" Then Return

        Try
            Dim blocks = CheckInvReader.ReadAll(LegacyDataPaths.CheckInv)
            Dim hit = blocks.FirstOrDefault(Function(b) String.Equals(If(b.CheckNumber, "").Trim(), checkNo, StringComparison.OrdinalIgnoreCase))

            lstInvoices.Items.Clear()

            If hit Is Nothing Then
                lblHeader.Text = $"No match for check #{checkNo}."
                Return
            End If

            lblHeader.Text = $"Customer: {hit.CustomerCode}    Check#: {hit.CheckNumber}    Date: {hit.DateText}    Amount: {hit.Amount:C}    Invoices: {hit.InvoiceCount}"

            For Each inv In hit.Invoices
                lstInvoices.Items.Add(inv)
            Next
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, "Find by Check #", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class