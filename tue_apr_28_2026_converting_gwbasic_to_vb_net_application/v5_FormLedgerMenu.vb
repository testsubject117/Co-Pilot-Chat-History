Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Globalization
Imports System.Text
Imports System.Windows.Forms

Public Class FormLedgerMenu
    Inherits Form

    Private ReadOnly _rtb As New RichTextBox()
    Private ReadOnly _timer As New Timer()

    ' Prompt flow for option (3)
    Private Enum PromptState
        None = 0
        Customer = 1
        CheckNumber = 2
        Year = 3
    End Enum

    Private _state As PromptState = PromptState.None
    Private _inputBuffer As String = ""

    Private _filterCustomer As String = ""
    Private _filterCheck As String = ""
    Private _filterYearText As String = ""

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        Text = "CHECKS"
        StartPosition = FormStartPosition.CenterParent
        Width = 900
        Height = 540
        KeyPreview = True

        ' DOS-like text surface
        _rtb.Dock = DockStyle.Fill
        _rtb.ReadOnly = True
        _rtb.BorderStyle = BorderStyle.None
        _rtb.BackColor = Color.Black
        _rtb.ForeColor = Color.White
        _rtb.Font = New Font("Consolas", 14.0F, FontStyle.Regular, GraphicsUnit.Point)
        _rtb.ScrollBars = RichTextBoxScrollBars.None
        _rtb.DetectUrls = False
        _rtb.TabStop = False
        _rtb.ShortcutsEnabled = False
        _rtb.Cursor = Cursors.Default

        Controls.Clear()
        Controls.Add(_rtb)

        ' Update time/date in header
        _timer.Interval = 1000
        AddHandler _timer.Tick, Sub() RenderScreen()
        _timer.Start()

        RenderScreen()
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        MyBase.OnFormClosed(e)
        Try
            _timer.Stop()
        Catch
        End Try
    End Sub

    Protected Overrides Sub OnKeyPress(e As KeyPressEventArgs)
        MyBase.OnKeyPress(e)

        ' If we're in prompt mode, capture typing for the underline fields
        If _state <> PromptState.None Then
            HandlePromptKeyPress(e)
            Return
        End If

        Dim ch As Char = Char.ToUpperInvariant(e.KeyChar)

        Select Case ch
            Case "1"c
                OpenAddChecks()
            Case "2"c
                OpenDeleteChecks()
            Case "3"c
                BeginViewChecksPromptFlow()
            Case "5"c
                OpenCompanyTotals()
            Case "6"c
                OpenAddOtherChecks()
            Case "7"c
                OpenDeleteOtherChecks()
            Case "8"c
                OpenViewOtherChecks()
            Case "9"c
                OpenFindByCheck()
            Case "0"c
                OpenFindByInvoice()
            Case "A"c
                OpenFindNotBalanced()
            Case "S"c
                OpenSalesEmployeesChecks()
            Case "Q"c
                Close()
        End Select
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)

        If e.KeyCode = Keys.Escape Then
            If _state <> PromptState.None Then
                CancelPromptFlow()
            Else
                Close()
            End If
            e.Handled = True
            Return
        End If

        ' Backspace in prompt mode is easier to handle here as well
        If _state <> PromptState.None AndAlso e.KeyCode = Keys.Back Then
            If _inputBuffer.Length > 0 Then
                _inputBuffer = _inputBuffer.Substring(0, _inputBuffer.Length - 1)
                RenderScreen()
            End If
            e.Handled = True
            Return
        End If

        If _state <> PromptState.None AndAlso e.KeyCode = Keys.Enter Then
            CommitPromptValueAndAdvance()
            e.Handled = True
            Return
        End If
    End Sub

    ' =========================
    ' Rendering
    ' =========================
    Private Sub RenderScreen()
        Dim sb As New StringBuilder()

        ' Header line: CHECKS + time + date (similar placement)
        ' We'll approximate DOS placement using padding.
        Dim timeText As String = DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture)
        Dim dateText As String = DateTime.Now.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture)

        ' "CHECKS" + spacing + time + spacing + date
        ' Adjust pad widths to match your font/window if desired
        sb.AppendLine(PadHeader("CHECKS", timeText, dateText))
        sb.AppendLine()

        sb.AppendLine(" (1) Add Checks to ledger & cash receipts")
        sb.AppendLine(" (2) Delete Checks")
        sb.AppendLine(" (3) View Checks")
        sb.AppendLine(" (5) View Companys Totals")
        sb.AppendLine(" (6) Add OTHER Checks")
        sb.AppendLine(" (7) Delete OTHER Checks")
        sb.AppendLine(" (8) View OTHER Checks")
        sb.AppendLine(" (9) Find a Check #")
        sb.AppendLine(" (0) Find an Invoice #")
        sb.AppendLine(" (A) Find Checks that don't balance.")
        sb.AppendLine(" (S) Sales Employee's checks")
        sb.AppendLine(" (Q) Quit to Main Menu")
        sb.AppendLine()
        sb.AppendLine()

        ' Prompt area (only shown after pressing 3)
        If _state <> PromptState.None Then
            sb.AppendLine(RenderPromptLine(PromptState.Customer, "Enter customer name [Enter = all customers] ? ", 24, _filterCustomer, If(_state = PromptState.Customer, _inputBuffer, "")))
            sb.AppendLine(RenderPromptLine(PromptState.CheckNumber, "Enter Check Number [Enter = All Checks] ? ", 24, _filterCheck, If(_state = PromptState.CheckNumber, _inputBuffer, "")))
            sb.AppendLine(RenderPromptLine(PromptState.Year, "Enter Year [Enter = All Years] ? ", 6, _filterYearText, If(_state = PromptState.Year, _inputBuffer, "")))
        End If

        _rtb.Text = sb.ToString()

        ' Color: make the word CHECKS yellow-ish like DOS
        ' We'll color just the first line "CHECKS" segment.
        Try
            _rtb.SelectAll()
            _rtb.SelectionColor = Color.White

            ' First line: color "CHECKS" in yellow
            _rtb.Select(0, "CHECKS".Length)
            _rtb.SelectionColor = Color.Gold

            _rtb.Select(0, 0)
        Catch
            ' ignore coloring failures
        End Try
    End Sub

    Private Shared Function PadHeader(title As String, timeText As String, dateText As String) As String
        ' Approximate a fixed layout across the top line.
        ' This doesn't need to be exact pixel-perfect; it's "DOS-like".
        Dim left As String = title

        ' target total chars (Consolas 14, typical width). Adjust if you want.
        Dim totalWidth As Integer = 60

        Dim right As String = $"{timeText}    {dateText}"
        Dim spaces As Integer = Math.Max(1, totalWidth - left.Length - right.Length)
        Return left & New String(" "c, spaces) & right
    End Function

    Private Function RenderPromptLine(lineState As PromptState,
                                      label As String,
                                      underlineLen As Integer,
                                      committedValue As String,
                                      activeBuffer As String) As String
        Dim valueToShow As String

        If _state > lineState Then
            valueToShow = committedValue
        ElseIf _state = lineState Then
            valueToShow = activeBuffer
        Else
            valueToShow = ""
        End If

        ' Underline field (DOS underscores)
        Dim underscores As String = New String("_"c, underlineLen)

        ' Put typed value over the underscores (simple overlay)
        Dim shown As String = valueToShow
        If shown.Length > underlineLen Then shown = shown.Substring(0, underlineLen)

        Dim field As String = underscores
        If shown.Length > 0 Then
            field = shown & underscores.Substring(shown.Length)
        End If

        Return label & field
    End Function

    ' =========================
    ' Prompt flow for View Checks
    ' =========================
    Private Sub BeginViewChecksPromptFlow()
        _state = PromptState.Customer
        _inputBuffer = ""

        _filterCustomer = ""
        _filterCheck = ""
        _filterYearText = ""

        RenderScreen()
    End Sub

    Private Sub CancelPromptFlow()
        _state = PromptState.None
        _inputBuffer = ""
        RenderScreen()
    End Sub

    Private Sub HandlePromptKeyPress(e As KeyPressEventArgs)
        ' Allow printable characters; Enter/Backspace handled in KeyDown
        Dim ch As Char = e.KeyChar

        If ch = ChrW(27) Then ' ESC
            CancelPromptFlow()
            e.Handled = True
            Return
        End If

        If Char.IsControl(ch) Then
            Return
        End If

        ' Conservative max lengths to match the underscore fields visually
        Dim maxLen As Integer = If(_state = PromptState.Year, 6, 24)
        If _inputBuffer.Length >= maxLen Then
            e.Handled = True
            Return
        End If

        _inputBuffer &= ch
        RenderScreen()
        e.Handled = True
    End Sub

    Private Sub CommitPromptValueAndAdvance()
        Select Case _state
            Case PromptState.Customer
                _filterCustomer = _inputBuffer.Trim()
                _inputBuffer = ""
                _state = PromptState.CheckNumber
                RenderScreen()

            Case PromptState.CheckNumber
                _filterCheck = _inputBuffer.Trim()
                _inputBuffer = ""
                _state = PromptState.Year
                RenderScreen()

            Case PromptState.Year
                _filterYearText = _inputBuffer.Trim()
                _inputBuffer = ""
                _state = PromptState.None
                RenderScreen()

                ' Now execute "View Checks" with filters.
                OpenViewChecksWithFilters(_filterCustomer, _filterCheck, _filterYearText)
        End Select
    End Sub

    ' =========================
    ' Actions (wire these to your existing forms)
    ' =========================
    Private Sub OpenAddChecks()
        MessageBox.Show("Add Checks to ledger & cash receipts is not wired up yet.", "CHECKS", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub OpenDeleteChecks()
        MessageBox.Show("Delete Checks is not wired up yet.", "CHECKS", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub OpenCompanyTotals()
        Using f As New FormLedgerCompanyTotals()
            f.ShowDialog(Me)
        End Using
    End Sub

    Private Sub OpenAddOtherChecks()
        MessageBox.Show("Add OTHER Checks is not wired up yet.", "CHECKS", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub OpenDeleteOtherChecks()
        MessageBox.Show("Delete OTHER Checks is not wired up yet.", "CHECKS", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub OpenViewOtherChecks()
        Using f As New FormOtherChecksView()
            f.ShowDialog(Me)
        End Using
    End Sub

    Private Sub OpenFindByCheck()
        Using f As New FormFindByCheckNumber()
            f.ShowDialog(Me)
        End Using
    End Sub

    Private Sub OpenFindByInvoice()
        Using f As New FormFindByInvoiceNumber()
            f.ShowDialog(Me)
        End Using
    End Sub

    Private Sub OpenFindNotBalanced()
        MessageBox.Show("Find Checks that don't balance is not wired up yet.", "CHECKS", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub OpenSalesEmployeesChecks()
        MessageBox.Show("Sales Employee's checks is not wired up yet.", "CHECKS", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub OpenViewChecksWithFilters(customer As String, checkNo As String, yearText As String)
        ' TODO: Wire filters into FormLedgerView once we know its API.
        ' For now open the normal view so the menu flow matches DOS.
        Using f As New FormLedgerView()
            f.ShowDialog(Me)
        End Using

        ' If FormLedgerView supports filters via properties/constructor, tell me how and I will update this.
        ' Example:
        ' Using f As New FormLedgerView(customer, checkNo, yearText)
        '   f.ShowDialog(Me)
        ' End Using
    End Sub

End Class