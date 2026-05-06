Option Strict Off
Option Explicit On

Imports System.Windows.Forms

Partial Public Class FormMainMenu

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        BuildMenu()
    End Sub

    Private Sub BuildMenu()
        flpLeft.Controls.Clear()
        flpRight.Controls.Clear()

        ' Left column
        AddMenuButton(flpLeft, "A", "Shop Card Generator")
        AddMenuButton(flpLeft, "B", "Invoice Generator")
        AddMenuButton(flpLeft, "C", "Checks and Cash Receipts")
        AddMenuButton(flpLeft, "D", "View Sales Journal")
        AddMenuButton(flpLeft, "E", "View Log Book")
        AddMenuButton(flpLeft, "F", "Price List Program")
        AddMenuButton(flpLeft, "G", "Print Records, Void Invoices")
        AddMenuButton(flpLeft, "H", "Quick Message Flashing")
        AddMenuButton(flpLeft, "I", "Backup Price List & Rolodex")
        AddMenuButton(flpLeft, "J", "Print Out Customers Actual Names")
        AddMenuButton(flpLeft, "K", "Cash Disbursements")
        AddMenuButton(flpLeft, "L", "Business Expenses Account")
        AddMenuButton(flpLeft, "M", "Quotation Form Generator")
        AddMenuButton(flpLeft, "N", "Rolodex")

        ' Right column
        AddMenuButton(flpRight, "O", "Copy Spec Index")
        AddMenuButton(flpRight, "P", "Entire Ledger Viewing")
        AddMenuButton(flpRight, "Q", "Word Processor")
        AddMenuButton(flpRight, "R", "Find Word Processor text")
        AddMenuButton(flpRight, "S", "Clean Keyboard")
        AddMenuButton(flpRight, "T", "Change Date or Time")
        AddMenuButton(flpRight, "U", "Phone Line for Dean")
        AddMenuButton(flpRight, "V", "Change Main Menu Message")
        AddMenuButton(flpRight, "W", "Leave a Phone Message")
        AddMenuButton(flpRight, "X", "Typewriter Mode")
        AddMenuButton(flpRight, "Y", "Ed Dean's Personal Backup")
        AddMenuButton(flpRight, "Z", "Personal Calendar")
        AddMenuButton(flpRight, "1", "Mileage Tracking")
        AddMenuButton(flpRight, "2", "Product Purchasing")
        AddMenuButton(flpRight, "3", "Miscellaneous Menu")
        AddMenuButton(flpRight, "4", "Add Entries to Log Book")
        AddMenuButton(flpRight, "+", "Employee Application Test")
        AddMenuButton(flpRight, "5", "VGA Color Fonts")
        AddMenuButton(flpRight, "6", "Cadmium Cards")
        AddMenuButton(flpRight, "7", "Emergency PAYROLL System")
    End Sub

    Private Sub AddMenuButton(panel As FlowLayoutPanel, key As String, text As String)
        Dim btn As New Button()
        btn.AutoSize = False
        btn.Width = panel.ClientSize.Width - 25
        btn.Height = 32
        btn.TextAlign = ContentAlignment.MiddleLeft
        btn.Text = $"({key}) {text}"
        btn.Tag = key
        AddHandler btn.Click, Sub() HandleMenuKey(key)
        panel.Controls.Add(btn)
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        ' Keep buttons fitting the panel width
        ResizeButtons(flpLeft)
        ResizeButtons(flpRight)
    End Sub

    Private Sub ResizeButtons(panel As FlowLayoutPanel)
        For Each c As Control In panel.Controls
            Dim btn = TryCast(c, Button)
            If btn IsNot Nothing Then
                btn.Width = panel.ClientSize.Width - 25
            End If
        Next
    End Sub

    Private Sub FormMainMenu_KeyPress(sender As Object, e As KeyPressEventArgs) Handles Me.KeyPress
        ' Hotkey behavior like INKEY$
        Dim ch As String = e.KeyChar.ToString()
        If ch = vbCr OrElse ch = vbLf Then Return
        HandleMenuKey(ch)
    End Sub

    Private Sub HandleMenuKey(ch As String)
        Dim key = ch

        ' Match original behavior: accept lowercase too
        If key.Length = 1 Then
            Dim up = key.ToUpperInvariant()
            Dim low = key.ToLowerInvariant()

            Select Case up
                Case "A" : NotYet("Shop Card Generator")
                Case "B" : NotYet("Invoice Generator")
                Case "C" : NotYet("Ledger (Checks/Cash Receipts)")
                Case "D" : NotYet("Sales Journal")
                Case "E" : NotYet("Log Book")
                Case "F" : NotYet("Price List")
                Case "G" : NotYet("Print/Void")
                Case "H" : NotYet("Quick Message Flashing")
                Case "I" : NotYet("Backup routine (we'll implement in .NET)")
                Case "J" : NotYet("Print customer actual names")
                Case "K" : NotYet("Cash Disbursements")
                Case "L" : NotYet("Business Expenses (password)")
                Case "M" : NotYet("Quotation")
                Case "N" : NotYet("Rolodex")
                Case "O" : NotYet("Copy Spec Index")
                Case "P" : NotYet("Entire Ledger Viewing")
                Case "Q" : NotYet("Word Processor")
                Case "R" : NotYet("Find Word Processor Text")
                Case "S" : NotYet("Clean Keyboard / Message")
                Case "T" : NotYet("Change Date or Time")
                Case "U" : NotYet("Phone Line for Dean")
                Case "V" : NotYet("Change Main Menu Message")
                Case "W" : NotYet("Leave Phone Message")
                Case "X" : NotYet("Typewriter Mode")
                Case "Y" : NotYet("Ed Dean Personal Backup")
                Case "Z" : NotYet("Personal Calendar")
            End Select

            Select Case key
                Case "1" : NotYet("Mileage Tracking")
                Case "2" : NotYet("Product Purchasing")
                Case "3" : NotYet("Miscellaneous Menu")
                Case "4" : NotYet("Add Entries to Log Book")
                Case "+" : NotYet("Employee Application Test")
                Case "5" : NotYet("VGA Color Fonts")
                Case "6" : NotYet("Cadmium Cards")
                Case "7" : NotYet("Emergency Payroll System")
            End Select
        End If
    End Sub

    Private Sub NotYet(feature As String)
        MessageBox.Show("Not implemented yet: " & feature, "Port status", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

End Class