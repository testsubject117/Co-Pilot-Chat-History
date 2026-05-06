Option Strict Off
Option Explicit On

Imports System
Imports System.Windows.Forms

Public Class FormLedgerMenu
    Inherits DosMenuFormBase

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        SetMenuTitle("CHECKS")

        BuildMenu()
    End Sub

    Private Sub BuildMenu()
        ClearMenu()

        ' Left column
        AddMenuButton(flpLeft, "1", "Add Checks to ledger & cash receipts", Sub() NotYet("Add Checks to ledger & cash receipts"))
        AddMenuButton(flpLeft, "2", "Delete Checks", Sub() NotYet("Delete Checks"))
        AddMenuButton(flpLeft, "3", "View Checks", Sub() OpenViewChecksPromptFlow())
        AddMenuButton(flpLeft, "5", "View Companys Totals", Sub()
                                                                Using f As New FormLedgerCompanyTotals()
                                                                    f.ShowDialog(Me)
                                                                End Using
                                                            End Sub)

        ' Right column
        AddMenuButton(flpRight, "6", "Add OTHER Checks", Sub() NotYet("Add OTHER Checks"))
        AddMenuButton(flpRight, "7", "Delete OTHER Checks", Sub() NotYet("Delete OTHER Checks"))
        AddMenuButton(flpRight, "8", "View OTHER Checks", Sub()
                                                              Using f As New FormOtherChecksView()
                                                                  f.ShowDialog(Me)
                                                              End Using
                                                          End Sub)
        AddMenuButton(flpRight, "9", "Find a Check #", Sub()
                                                          Using f As New FormFindByCheckNumber()
                                                              f.ShowDialog(Me)
                                                          End Using
                                                      End Sub)
        AddMenuButton(flpRight, "0", "Find an Invoice #", Sub()
                                                              Using f As New FormFindByInvoiceNumber()
                                                                  f.ShowDialog(Me)
                                                              End Using
                                                          End Sub)
        AddMenuButton(flpRight, "A", "Find Checks that don't balance.", Sub() NotYet("Find Checks that don't balance"))
        AddMenuButton(flpRight, "S", "Sales Employee's checks", Sub() NotYet("Sales Employee's checks"))
        AddMenuButton(flpRight, "Q", "Quit to Main Menu", Sub() Me.Close())
    End Sub

    ' This reproduces the DOS “prompt chain” behavior, but using small input dialogs.
    ' It keeps the *look* consistent with Main Menu (buttons + header), while still following the flow.
    Private Sub OpenViewChecksPromptFlow()
        Dim cust As String = Prompt("Enter customer name [Enter = all customers] ?", "")
        If cust Is Nothing Then Return ' cancelled

        Dim chk As String = Prompt("Enter Check Number [Enter = All Checks] ?", "")
        If chk Is Nothing Then Return

        Dim yr As String = Prompt("Enter Year [Enter = All Years] ?", "")
        If yr Is Nothing Then Return

        ' TODO: pass these into FormLedgerView when it supports filters.
        Using f As New FormLedgerView()
            f.ShowDialog(Me)
        End Using
    End Sub

    Private Function Prompt(promptText As String, defaultValue As String) As String
        Using f As New FormPrompt(promptText, defaultValue)
            Dim dr = f.ShowDialog(Me)
            If dr <> DialogResult.OK Then Return Nothing
            Return f.Value
        End Using
    End Function

    ' Small reusable prompt dialog (keeps this menu clean)
    Private NotInheritable Class FormPrompt
        Inherits Form

        Private ReadOnly _txt As New TextBox()
        Public ReadOnly Property Value As String
            Get
                Return _txt.Text
            End Get
        End Property

        Public Sub New(promptText As String, defaultValue As String)
            Me.Text = "CHECKS"
            Me.StartPosition = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MinimizeBox = False
            Me.MaximizeBox = False
            Me.ShowInTaskbar = False
            Me.ClientSize = New Drawing.Size(620, 140)

            Dim layout As New TableLayoutPanel() With {
                .Dock = DockStyle.Fill,
                .Padding = New Padding(10),
                .ColumnCount = 1,
                .RowCount = 3
            }
            layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))

            Dim lbl As New Label() With {.AutoSize = True, .Text = promptText}
            _txt.Dock = DockStyle.Top
            _txt.Text = defaultValue

            Dim buttons As New FlowLayoutPanel() With {.Dock = DockStyle.Fill, .FlowDirection = FlowDirection.RightToLeft, .AutoSize = True}
            Dim ok As New Button() With {.Text = "OK", .DialogResult = DialogResult.OK, .AutoSize = True}
            Dim cancel As New Button() With {.Text = "Cancel", .DialogResult = DialogResult.Cancel, .AutoSize = True}
            buttons.Controls.Add(ok)
            buttons.Controls.Add(cancel)

            layout.Controls.Add(lbl, 0, 0)
            layout.Controls.Add(_txt, 0, 1)
            layout.Controls.Add(buttons, 0, 2)

            Me.Controls.Add(layout)
            Me.AcceptButton = ok
            Me.CancelButton = cancel
        End Sub
    End Class

End Class