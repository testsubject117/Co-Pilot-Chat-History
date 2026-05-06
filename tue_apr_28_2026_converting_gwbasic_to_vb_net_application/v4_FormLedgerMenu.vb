Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Windows.Forms

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

        ' Enable hotkeys
        Me.KeyPreview = True

        Dim flp As New FlowLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.TopDown,
            .WrapContents = False,
            .AutoScroll = True,
            .Padding = New Padding(12)
        }

        ' Menu buttons: centered + bold + black/white
        ConfigureMenuButton(btnViewChecks, "(1) View Checks")
        ConfigureMenuButton(btnCompanyTotals, "(5) View Company Totals")
        ConfigureMenuButton(btnOtherChecks, "(8) View OTHER Checks")
        ConfigureMenuButton(btnFindByCheck, "(9) Find by Check #")
        ConfigureMenuButton(btnFindByInvoice, "(0) Find by Invoice #")

        ' Close button: small + grey + (Esc) label
        ConfigureCloseButton(btnClose, "(Esc) Close")

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

        AddHandler btnFindByCheck.Click,
            Sub()
                Using f As New FormFindByCheckNumber()
                    f.ShowDialog(Me)
                End Using
            End Sub

        AddHandler btnFindByInvoice.Click,
            Sub()
                Using f As New FormFindByInvoiceNumber()
                    f.ShowDialog(Me)
                End Using
            End Sub

        AddHandler btnClose.Click, Sub() Close()

        ' Add the menu buttons
        flp.Controls.Add(btnViewChecks)
        flp.Controls.Add(btnCompanyTotals)
        flp.Controls.Add(btnOtherChecks)
        flp.Controls.Add(btnFindByCheck)
        flp.Controls.Add(btnFindByInvoice)

        ' Put Close in its own row/panel so it can be centered/right-aligned cleanly
        Dim closeRow As New FlowLayoutPanel() With {
            .Width = 460,
            .Height = 40,
            .Margin = New Padding(3, 10, 3, 0),
            .Padding = New Padding(0),
            .WrapContents = False,
            .FlowDirection = FlowDirection.RightToLeft,
            .AutoSize = False
        }
        closeRow.Controls.Add(btnClose)

        flp.Controls.Add(closeRow)

        Controls.Clear()
        Controls.Add(flp)
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)

        Select Case e.KeyCode
            Case Keys.D1, Keys.NumPad1
                btnViewChecks.PerformClick()
                e.Handled = True

            Case Keys.D5, Keys.NumPad5
                btnCompanyTotals.PerformClick()
                e.Handled = True

            Case Keys.D8, Keys.NumPad8
                btnOtherChecks.PerformClick()
                e.Handled = True

            Case Keys.D9, Keys.NumPad9
                btnFindByCheck.PerformClick()
                e.Handled = True

            Case Keys.D0, Keys.NumPad0
                btnFindByInvoice.PerformClick()
                e.Handled = True

            Case Keys.Escape
                Close()
                e.Handled = True
        End Select
    End Sub

    Private Shared Sub ConfigureMenuButton(btn As Button, text As String)
        btn.Text = text
        btn.Width = 460
        btn.Height = 44
        btn.Margin = New Padding(3, 3, 3, 8)

        ' Center + bold (per request)
        btn.TextAlign = ContentAlignment.MiddleCenter
        btn.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold, GraphicsUnit.Point)

        ' Black background, white text
        btn.UseVisualStyleBackColor = False
        btn.BackColor = Color.Black
        btn.ForeColor = Color.White
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderColor = Color.DimGray
        btn.FlatAppearance.BorderSize = 1

        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(32, 32, 32)
        btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(64, 64, 64)
    End Sub

    Private Shared Sub ConfigureCloseButton(btn As Button, text As String)
        btn.Text = text

        ' Match the "small grey Close" style used in the code-built forms
        btn.AutoSize = True
        btn.Height = 32
        btn.Margin = New Padding(0) ' closeRow handles spacing

        btn.TextAlign = ContentAlignment.MiddleCenter
        btn.Font = New Font("Segoe UI", 10.0F, FontStyle.Regular, GraphicsUnit.Point)

        ' Grey background, black text (standard button rendering)
        btn.UseVisualStyleBackColor = True
        btn.BackColor = SystemColors.Control
        btn.ForeColor = Color.Black
        btn.FlatStyle = FlatStyle.Standard
    End Sub

End Class