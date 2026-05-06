Option Strict Off
Option Explicit On

Imports System
Imports System.Drawing
Imports System.Windows.Forms

Partial Public Class FormMainMenu

    ' NOTE:
    ' - This file is intended for a "true GUI menu" (no terminal emulation).
    ' - It assumes FormMainMenu.Designer.vb defines these controls:
    '     Friend WithEvents flpLeft As FlowLayoutPanel
    '     Friend WithEvents flpRight As FlowLayoutPanel
    '     Friend WithEvents tlpRoot As TableLayoutPanel
    ' - If your Designer currently does NOT place flpLeft/flpRight inside tlpRoot,
    '   this code will do it at runtime in OnLoad.

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        ' Ensure KeyPress is received at the form level
        Me.KeyPreview = True

        ' Ensure layout is correct even if the Designer currently adds controls directly to the form.
        EnsureLayout()

        ' Build the menu buttons
        BuildMenu()
    End Sub

    Private Sub EnsureLayout()
        ' Make tlpRoot the only root control, and host left/right panels inside it.
        ' This avoids "Dock=Fill" overlap and keeps layout predictable.

        If tlpRoot Is Nothing Then Return
        If flpLeft Is Nothing OrElse flpRight Is Nothing Then Return

        ' Configure table: 2 columns, 1 row
        tlpRoot.SuspendLayout()

        tlpRoot.Dock = DockStyle.Fill
        tlpRoot.ColumnStyles.Clear()
        tlpRoot.RowStyles.Clear()

        tlpRoot.ColumnCount = 2
        tlpRoot.RowCount = 1
        tlpRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        tlpRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))

        ' Configure panels
        ConfigureMenuPanel(flpLeft)
        ConfigureMenuPanel(flpRight)

        ' If panels are not already children of tlpRoot, add them.
        If flpLeft.Parent IsNot tlpRoot OrElse flpRight.Parent IsNot tlpRoot Then
            tlpRoot.Controls.Clear()
            tlpRoot.Controls.Add(flpLeft, 0, 0)
            tlpRoot.Controls.Add(flpRight, 1, 0)
        Else
            ' Ensure correct cell placement
            tlpRoot.SetCellPosition(flpLeft, New TableLayoutPanelCellPosition(0, 0))
            tlpRoot.SetCellPosition(flpRight, New TableLayoutPanelCellPosition(1, 0))
        End If

        ' Ensure tlpRoot is the only top-level hosted control
        If Not Me.Controls.Contains(tlpRoot) Then
            Me.Controls.Add(tlpRoot)
        End If

        For i As Integer = Me.Controls.Count - 1 To 0 Step -1
            Dim c = Me.Controls(i)
            If c IsNot tlpRoot Then
                Me.Controls.RemoveAt(i)
            End If
        Next

        tlpRoot.ResumeLayout(performLayout:=True)
    End Sub

    Private Sub ConfigureMenuPanel(panel As FlowLayoutPanel)
        panel.SuspendLayout()

        panel.Dock = DockStyle.Fill
        panel.AutoScroll = True
        panel.FlowDirection = FlowDirection.TopDown
        panel.WrapContents = False
        panel.Padding = New Padding(10)

        panel.ResumeLayout(performLayout:=True)
    End Sub

    Private Sub BuildMenu()
        flpLeft.Controls.Clear()
        flpRight.Controls.Clear()

        ' Optional: add headers
        flpLeft.Controls.Add(MakeHeaderLabel("Main Menu (Left)"))
        flpRight.Controls.Add(MakeHeaderLabel("Main Menu (Right)"))

        ' Left column (A..N + K/L etc, mirroring your DOS screen)
        AddMenuButton(flpLeft, "A", "Shop Card Generator")
        AddMenuButton(flpLeft, "B", "Invoice Generator")
        AddMenuButton(flpLeft, "C", "Checks and Cash Receipts")
        AddMenuButton(flpLeft, "D", "View Sales Journal")
        AddMenuButton(flpLeft, "E", "View Log Book")
        AddMenuButton(flpLeft, "F", "Price List Program")
        AddMenuButton(flpLeft, "G", "Print Records / Void Invoices")
        AddMenuButton(flpLeft, "H", "Quick Message Flashing")
        AddMenuButton(flpLeft, "I", "Backup Price List & Rolodex")
        AddMenuButton(flpLeft, "J", "Print Out Customers Actual Names")
        AddMenuButton(flpLeft, "K", "Cash Disbursements")
        AddMenuButton(flpLeft, "L", "Business Expenses Account")
        AddMenuButton(flpLeft, "M", "Quotation Form Generator")
        AddMenuButton(flpLeft, "N", "Rolodex")

        ' Right column (O..Z, 1..7, +)
        AddMenuButton(flpRight, "O", "Copy Spec Index")
        AddMenuButton(flpRight, "P", "Entire Ledger Viewing")
        AddMenuButton(flpRight, "Q", "Word Processor")
        AddMenuButton(flpRight, "R", "Find Word Processor Text")
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

        ResizeButtonsToPanel(flpLeft)
        ResizeButtonsToPanel(flpRight)
    End Sub

    Private Function MakeHeaderLabel(text As String) As Label
        Dim lbl As New Label()
        lbl.AutoSize = False
        lbl.Height = 32
        lbl.Dock = DockStyle.Top
        lbl.TextAlign = ContentAlignment.MiddleLeft
        lbl.Font = New Font(Me.Font, FontStyle.Bold)
        lbl.Text = text
        lbl.Margin = New Padding(3, 3, 3, 8)
        Return lbl
    End Function

    Private Sub AddMenuButton(panel As FlowLayoutPanel, key As String, text As String)
        Dim btn As New Button()

        btn.AutoSize = False
        btn.Height = 34
        btn.TextAlign = ContentAlignment.MiddleLeft
        btn.Text = $"({key}) {text}"
        btn.Tag = key
        btn.Margin = New Padding(3, 3, 3, 6)

        ' Visual tweak for readability
        btn.UseVisualStyleBackColor = True

        AddHandler btn.Click, Sub(sender, args) HandleMenuKey(CStr(btn.Tag))

        panel.Controls.Add(btn)
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        ResizeButtonsToPanel(flpLeft)
        ResizeButtonsToPanel(flpRight)
    End Sub

    Private Sub ResizeButtonsToPanel(panel As FlowLayoutPanel)
        If panel Is Nothing Then Return

        ' Calculate width allowing for scroll bar + padding
        Dim targetWidth As Integer = panel.ClientSize.Width - panel.Padding.Left - panel.Padding.Right - 5
        If targetWidth < 50 Then targetWidth = 50

        For Each c As Control In panel.Controls
            Dim btn = TryCast(c, Button)
            Dim lbl = TryCast(c, Label)

            If btn IsNot Nothing Then
                btn.Width = targetWidth
            ElseIf lbl IsNot Nothing Then
                lbl.Width = targetWidth
            End If
        Next
    End Sub

    Private Sub FormMainMenu_KeyPress(sender As Object, e As KeyPressEventArgs) Handles Me.KeyPress
        ' Hotkey behavior (similar to INKEY$)
        Dim ch As String = e.KeyChar.ToString()
        If ch = vbCr OrElse ch = vbLf Then Return
        HandleMenuKey(ch)
    End Sub

    Private Sub HandleMenuKey(ch As String)
        If String.IsNullOrEmpty(ch) Then Return

        ' Normalize letter keys to uppercase for matching.
        Dim up As String = ch
        If up.Length = 1 AndAlso Char.IsLetter(up(0)) Then
            up = up.ToUpperInvariant()
        End If

        Select Case up
            Case "A" : NotYet("Shop Card Generator")
            Case "B" : NotYet("Invoice Generator")
            Case "C" : NotYet("Checks and Cash Receipts (LEDGER)")
            Case "D" : NotYet("View Sales Journal (SALES)")
            Case "E" : NotYet("View Log Book (LOGBOOK)")
            Case "F" : NotYet("Price List Program (plist)")
            Case "G" : NotYet("Print/Void Invoices (BOOT)")
            Case "H" : NotYet("Quick Message Flashing")
            Case "I" : NotYet("Backup Price List & Rolodex (implement in .NET)")
            Case "J" : NotYet("Print Out Customers Actual Names (spool real names)")
            Case "K" : NotYet("Cash Disbursements (BILL)")
            Case "L" : NotYet("Business Expenses Account (password)")
            Case "M" : NotYet("Quotation Form Generator (QUOTE)")
            Case "N" : NotYet("Rolodex (PHONE)")
            Case "O" : NotYet("Copy Spec Index")
            Case "P" : NotYet("Entire Ledger Viewing (ENTIRE)")
            Case "Q" : NotYet("Word Processor")
            Case "R" : NotYet("Find Word Processor Text")
            Case "S" : NotYet("Clean Keyboard / Message")
            Case "T" : NotYet("Change Date or Time")
            Case "U" : NotYet("Phone Line for Dean")
            Case "V" : NotYet("Change Main Menu Message")
            Case "W" : NotYet("Leave a Phone Message")
            Case "X" : NotYet("Typewriter Mode")
            Case "Y" : NotYet("Ed Dean's Personal Backup")
            Case "Z" : NotYet("Personal Calendar")

            Case "1" : NotYet("Mileage Tracking")
            Case "2" : NotYet("Product Purchasing")
            Case "3" : NotYet("Miscellaneous Menu")
            Case "4" : NotYet("Add Entries to Log Book")
            Case "+" : NotYet("Employee Application Test")
            Case "5" : NotYet("VGA Color Fonts")
            Case "6" : NotYet("Cadmium Cards")
            Case "7" : NotYet("Emergency PAYROLL System")

            Case Else
                ' ignore unknown keys
        End Select
    End Sub

    Private Sub NotYet(feature As String)
        MessageBox.Show("Not implemented yet: " & feature, "Port status", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

End Class