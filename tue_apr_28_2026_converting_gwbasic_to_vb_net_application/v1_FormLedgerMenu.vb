Option Strict On
Option Explicit On

Public Class FormLedgerMenu

    Private ReadOnly btnViewChecks As New Button()
    Private ReadOnly btnCompanyTotals As New Button()
    Private ReadOnly btnOtherChecks As New Button()
    Private ReadOnly btnFindByCheck As New Button()
    Private ReadOnly btnFindByInvoice As New Button()
    Private ReadOnly btnClose As New Button()

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        Text = "Ledger Menu"
        Width = 520
        Height = 420
        StartPosition = FormStartPosition.CenterParent

        Dim flp As New FlowLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.TopDown,
            .WrapContents = False,
            .AutoScroll = True,
            .Padding = New Padding(12)
        }

        Configure(btnViewChecks, "View Checks (LEDGER.CUR)")
        Configure(btnCompanyTotals, "View Company Totals (Option 5)")
        Configure(btnOtherChecks, "View OTHER Checks (Option 8)")
        Configure(btnFindByCheck, "Find by Check # (Option 9)")
        Configure(btnFindByInvoice, "Find by Invoice # (Option 0)")
        Configure(btnClose, "Close")

        AddHandler btnViewChecks.Click,
            Sub()
                Using f As New FormLedgerView()
                    f.ShowDialog(Me)
                End Using
            End Sub

        AddHandler btnCompanyTotals.Click,
            Sub()
                Using f As New FormLedgerCompanyTotals()
                    f.ShowDialog(Me)
                End Using
            End Sub

        AddHandler btnOtherChecks.Click,
            Sub()
                Using f As New FormOtherChecksView()
                    f.ShowDialog(Me)
                End Using
            End Sub

        ' These two forms we will add AFTER you paste a sample of CHECK.INV
        AddHandler btnFindByCheck.Click, Sub() MessageBox.Show(Me, "Next: implement CHECK.INV parser + Find by Check # form.")
        AddHandler btnFindByInvoice.Click, Sub() MessageBox.Show(Me, "Next: implement CHECK.INV parser + Find by Invoice # form.")

        AddHandler btnClose.Click, Sub() Close()

        flp.Controls.Add(btnViewChecks)
        flp.Controls.Add(btnCompanyTotals)
        flp.Controls.Add(btnOtherChecks)
        flp.Controls.Add(btnFindByCheck)
        flp.Controls.Add(btnFindByInvoice)
        flp.Controls.Add(btnClose)

        Controls.Add(flp)
    End Sub

    Private Shared Sub Configure(btn As Button, text As String)
        btn.Text = text
        btn.Width = 460
        btn.Height = 44
        btn.Margin = New Padding(3, 3, 3, 8)
        btn.TextAlign = ContentAlignment.MiddleLeft
        btn.BackColor = Color.Black
        btn.ForeColor = Color.White
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderColor = Color.DimGray
    End Sub

End Class