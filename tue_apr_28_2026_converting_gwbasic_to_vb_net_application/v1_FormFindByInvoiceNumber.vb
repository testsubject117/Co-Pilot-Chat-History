Option Strict On
Option Explicit On

Imports System.Linq

Public Class FormFindByInvoiceNumber

    Private ReadOnly txtInvoice As New TextBox()
    Private ReadOnly btnFind As New Button()
    Private ReadOnly lblResult As New Label()

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        Text = "Find by Invoice # (CHECK.INV)"
        Width = 750
        Height = 300
        StartPosition = FormStartPosition.CenterParent

        Dim top As New FlowLayoutPanel() With {.Dock = DockStyle.Top, .Height = 45, .Padding = New Padding(10)}
        top.Controls.Add(New Label() With {.Text = "Invoice #:", .AutoSize = True, .Padding = New Padding(0, 10, 0, 0)})
        txtInvoice.Width = 150
        top.Controls.Add(txtInvoice)
        btnFind.Text = "Find"
        top.Controls.Add(btnFind)

        lblResult.Dock = DockStyle.Fill
        lblResult.Padding = New Padding(10)
        lblResult.Text = "Enter an invoice # and click Find."

        Controls.Add(lblResult)
        Controls.Add(top)

        AddHandler btnFind.Click, Sub() DoFind()
        AddHandler txtInvoice.KeyDown,
            Sub(sender, args)
                If args.KeyCode = Keys.Enter Then
                    args.SuppressKeyPress = True
                    DoFind()
                End If
            End Sub
    End Sub

    Private Sub DoFind()
        Dim invNo = txtInvoice.Text.Trim()
        If invNo = "" Then Return

        Try
            Dim blocks = CheckInvReader.ReadAll(LegacyDataPaths.CheckInv)

            Dim hit = blocks.FirstOrDefault(
                Function(b) b.Invoices IsNot Nothing AndAlso b.Invoices.Any(Function(x) String.Equals(x.Trim(), invNo, StringComparison.OrdinalIgnoreCase))
            )

            If hit Is Nothing Then
                lblResult.Text = $"No match for invoice #{invNo}."
                Return
            End If

            lblResult.Text =
                $"Invoice {invNo} was paid by:" & Environment.NewLine &
                $"Customer: {hit.CustomerCode}" & Environment.NewLine &
                $"Check #:  {hit.CheckNumber}" & Environment.NewLine &
                $"Date:     {hit.DateText}" & Environment.NewLine &
                $"Amount:   {hit.Amount:C}" & Environment.NewLine &
                $"Invoices on that check: {hit.InvoiceCount}"
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, "Find by Invoice #", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class