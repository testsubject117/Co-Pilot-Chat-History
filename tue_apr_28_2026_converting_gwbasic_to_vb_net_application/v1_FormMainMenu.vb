Option Strict Off
Option Explicit On

Imports System.IO
Imports System.Text
Imports System.Windows.Forms

Public Class FormMainMenu

    Private WithEvents tmr As New Timer()

    ' --- "screen" state ---
    Private scr As GwScreen

    ' --- translated globals (keep names close to GWBASIC) ---
    Private X As Double
    Private Q$ As String
    Private A1$ As String
    Private AA$ As String
    Private STR1$ As String
    Private STR2$ As String
    Private STR3$ As String
    Private STR1 As Double
    Private FLAGG As Double
    Private BLANK As Double
    Private BLANK3 As Double
    Private QWER As Double
    Private MESS$ As String
    Private TODAY$ As String
    Private JOKE As Double
    Private JN As Double

    ' For menu loop timing
    Private DELAY As Double
    Private running As Boolean = False

    ' UI controls
    Private rtb As New RichTextBox()

    Public Sub New()
        Me.Text = "MAINMENU (GWBASIC port)"
        Me.Width = 1000
        Me.Height = 700

        rtb.Dock = DockStyle.Fill
        rtb.Font = New Drawing.Font("Consolas", 12.0F)
        rtb.ReadOnly = True
        rtb.Multiline = True
        rtb.BackColor = Drawing.Color.Black
        rtb.ForeColor = Drawing.Color.LightGray
        rtb.ScrollBars = RichTextBoxScrollBars.Vertical

        Me.Controls.Add(rtb)

        scr = New GwScreen(rtb, cols:=80, rows:=25)

        ' Timer to emulate INKEY$ loop without blocking UI
        tmr.Interval = 50
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        If Not running Then
            running = True
            MainMenu_20_StartUp()     ' start roughly at line 20 (CLEAR:RESET...)
            MainMenu_490_DrawMenu()   ' jump to the "main loop" / screen draw
            tmr.Start()
        End If
    End Sub

    Private Sub FormMainMenu_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        ' Route keystrokes into a single-character buffer like INKEY$
        Dim ch As String = GwRuntime.KeyEventToInkeyChar(e)
        If ch <> "" Then
            GwRuntime.PushKey(ch)
        End If
    End Sub

    Private Sub tmr_Tick(sender As Object, e As EventArgs) Handles tmr.Tick
        ' This simulates the tight INKEY$/TIMER loop around 680..1130
        Try
            MainMenu_680_LoopTick()
        Catch ex As Exception
            ' Replace ON ERROR GOTO 400 with VB.NET handling
            scr.PrintLine("")
            scr.PrintLine("VB.NET Exception: " & ex.Message)
        End Try
    End Sub

    ' ============================================================
    ' ============= TRANSLATION (label-style subs) ===============
    ' ============================================================

    ' (Lines 20..90-ish: init)
    Private Sub MainMenu_20_StartUp()
        ' 20 CLEAR:RESET:COLOR 7,0:RANDOMIZE TIMER
        GwRuntime.Reset()
        scr.Color(fg:=7, bg:=0)
        Randomize(Timer)

        ' 30 FOR X=1 TO 10:Q$=INKEY$:NEXT X
        For X = 1 To 10
            Q$ = GwRuntime.InKey()
        Next

        ' 40 KEY OFF
        ' 50..70 KEY X,""
        ' 80.. stuff - ignored in GUI (function keys macros)

        ' 170 ON ERROR GOTO 400 -> handled by Try/Catch in Tick

        ' 190 OPEN "NOTES-4.ED" FOR INPUT AS 1:INPUT#1,A1$:CLOSE 1:AA$=A1$
        Try
            A1$ = GwRuntime.ReadFirstLine("NOTES-4.ED")
        Catch
            A1$ = ""
        End Try
        AA$ = A1$

        ' 210 STR1$=... then overwritten by STR1$=AA$ in original
        STR1$ = AA$
        STR1 = 0
        STR2$ = New String(" "c, 80)
        STR3$ = STR1$
        FLAGG = 0

        ' 250 CLS:PRINT:GOTO 290
        scr.Cls()
        scr.PrintLine("")
        ' goto 290 happens in next method
        MainMenu_290_LoadDates()
    End Sub

    Private Sub MainMenu_290_LoadDates()
        ' 290 OPEN "CURRENT.DTE" ... INPUT#1,Q$
        Try
            Q$ = GwRuntime.ReadFirstLine("CURRENT.DTE")
        Catch
            Q$ = ""
        End Try

        ' 295 OPEN "SUBLIMAL.MES" ... INPUT#1,MESS$
        Try
            MESS$ = GwRuntime.ReadFirstLine("SUBLIMAL.MES")
        Catch
            MESS$ = ""
        End Try

        ' 300 IF Q$="PLEASE DONT BEEP MAIN MENU" THEN 490
        If Q$ = "PLEASE DONT BEEP MAIN MENU" Then
            Exit Sub
        End If

        ' 310 OPEN "CURRENT.DTE" FOR OUTPUT ... WRITE#1,DATE$
        GwRuntime.WriteLine("CURRENT.DTE", DateTime.Now.ToString("M-d-yyyy"))

        ' 315 IF VAL(RIGHT$(DATE$,4))<1992 THEN 330
        Dim nowYear As Integer = DateTime.Now.Year
        If nowYear < 1992 Then
            MainMenu_330_WarnWrongDate(savedOtherDate:=Q$)
            Exit Sub
        End If

        ' 320 IF DATE$=Q$ THEN 490
        Dim todayStr As String = DateTime.Now.ToString("M-d-yyyy")
        If todayStr = Q$ Then
            Exit Sub
        End If

        ' else goto 330
        MainMenu_330_WarnWrongDate(savedOtherDate:=Q$)
    End Sub

    Private Sub MainMenu_330_WarnWrongDate(savedOtherDate As String)
        scr.Cls()
        scr.PrintLine("")
        scr.Color(16, 1)
        scr.Print("WARNING: ")
        scr.Color(7, 0)
        scr.PrintLine("One or more of the computers has the Wrong Date.")
        scr.PrintLine("          Please find out which computers date is wrong and fix it.")
        scr.PrintLine("")
        scr.PrintLine("This Computers Date is " & DateTime.Now.ToString("M-d-yyyy"))
        scr.PrintLine("Another Computers Date is " & savedOtherDate)
        scr.PrintLine("")
        scr.PrintLine("Hit Q to go to main menu")

        ' GWBASIC loop 360..390 with SOUND beeps. We’ll just wait for Q in the normal loop.
        ' Store a state flag by setting Q$ = "" and letting menu loop proceed.
    End Sub

    ' (Line 490: draw menu)
    Private Sub MainMenu_490_DrawMenu()
        ' 490 GOSUB 2710
        MainMenu_2710_CalcDayOfWeek()

        scr.Color(12, 0)
        BLANK = Timer

        scr.Cls()

        scr.PrintLine("ÖÒ· ÖÄ· Ò Ö·Ö ÖÒ· ÖÄ· Ö·Ö · Ö ")
        scr.PrintLine("ººº ÇÄ¶ º ººº ººº ÇÄ ººº º º " & TODAY$ & " " & DateTime.Now.ToString("M-d-yyyy"))
        scr.Color(10, 0)
        scr.PrintLine("½ Ó ½ Ó Ð ½Ó½ ½ Ó ÓÄ½ ½Ó½ ÓÄ½ ")
        scr.Color(7, 0)

        scr.PrintLine("(A) Shop Card Generator $ % (O) Copy Spec Index")
        scr.PrintLine("(B) Invoice Generator * (P) Entire Ledger Viewing")
        scr.PrintLine("(C) Checks and Cash Reciepts (Q) Word Processor")
        scr.PrintLine("(D) View Sales Journal (R) Find Word Processor text")
        scr.PrintLine("(E) View Log Book (S) Clean KeyBoard")
        scr.PrintLine("(F) Price List Program (T) Change Date or Time")
        scr.PrintLine("(G) Print Records, Void Invoices (U) Phone Line for Dean")
        scr.PrintLine("(H) Quick Message Flashing (V) Change Main Menu Message")
        scr.PrintLine("(I) Backup Price List & Rolodex (W) Leave a Phone Message")
        scr.PrintLine("(J) Print Out Customers Actual Names (X) Typewriter Mode")

        If TODAY$ = "Tuesday" OrElse TODAY$ = "Friday" Then
            ' emulate COLOR 0,11
            scr.Color(0, 11)
        End If
        scr.Print("(K) Cash Disbursements")
        If TODAY$ = "Tuesday" OrElse TODAY$ = "Friday" Then
            scr.Print(" DEPOSIT TODAY")
        End If
        scr.PrintLine("")
        scr.Color(7, 0)

        scr.PrintLine("                                            (Y) Ed Dean's Personal Backup")
        scr.PrintLine("(L) Business Expenses Account  (Z) Personal Calendar")
        scr.PrintLine("(M) Quotation Form Generator (1) Mileage Tracking")
        scr.PrintLine("(N) Rolodex (2) Product Purchasing")
        scr.PrintLine("(3) Miscellaneous Menu (4) Add Entrys to Log Book")
        scr.PrintLine("(+) Employee Application Test (5) VGA Color Fonts")
        scr.PrintLine("(6) Cadmium Cards (7) Emergency PAYROLL System")
        scr.PrintLine("")
        scr.Print(GwRuntime.ChrW(30) & "TYPE A LETTER ")

        ' prime loop values
        X = 0
    End Sub

    ' (This simulates the 680..1130 loop without blocking)
    Private Sub MainMenu_680_LoopTick()
        ' 680 Q$=INKEY$:DELAY=TIMER
        Q$ = GwRuntime.InKey()
        DELAY = Timer

        ' 690 IF TIMER-DELAY<.1 ... THEN 690
        ' In event loop, we simply allow tick pacing.

        ' 700 status line STR2$ and marquee logic
        QWER = QWER + 1
        If QWER = 40 Then
            QWER = 0
            scr.Locate(1, 34)
            scr.Print(MESS$)
            scr.Locate(1, 34)
            scr.Print(New String(" "c, 79 - 33))
        End If

        STR1 = STR1 + 1
        If STR1 > Len(STR1$) AndAlso FLAGG = 0 Then
            FLAGG = 1
            STR1$ = New String(" "c, 80)
            STR1 = 1
        End If
        If STR1 > Len(STR1$) AndAlso FLAGG = 1 Then
            FLAGG = 0
            STR1$ = STR3$
            STR1 = 1
        End If

        If STR2$ Is Nothing Then STR2$ = New String(" "c, 80)
        STR2$ = Right(STR2$, 79) & Mid(STR1$, CInt(STR1), 1)

        scr.Locate(25, 1)
        scr.Color(0, 7)
        scr.Print(STR2$)
        scr.Color(7, 0)

        ' 740 time display
        scr.Locate(2, 70)
        scr.Print(DateTime.Now.ToString("HH:mm:ss"))

        ' 750 screen blanking timeout
        If Timer - BLANK > 604 Then
            ' goto 1490 (not implemented here)
            ' You can add a "blanking mode" overlay later.
        End If

        ' 780 IF Q$="" THEN 680
        If Q$ = "" Then Exit Sub

        ' 790 BLANK=TIMER and echo keystroke
        BLANK = Timer
        scr.Color(0, 7)
        scr.Locate(21, 18)
        scr.Print(Q$)
        scr.Color(7, 0)

        ' --- menu actions (translate the IF chain) ---
        Select Case Q$
            Case "F" : GwRuntime.NotImplemented("CHAIN ""plist""")
            Case "G" : GwRuntime.NotImplemented("CHAIN ""BOOT""")
            Case "I" : GwRuntime.NotImplemented("GOTO 1720 backup routine") ' later convert into a Sub
            Case "A" : GwRuntime.NotImplemented("SHELL ""QB /RUN SHOPCARD /MBF""")
            Case "%" : GwRuntime.NotImplemented("SHELL ""QB /RUN SHOPCRD2""")
            Case "$" : GwRuntime.NotImplemented("CHAIN ""S""")
            Case "B" : GwRuntime.NotImplemented("SHELL ""QB /RUN INVOICE /MBF""")
            Case "C" : GwRuntime.NotImplemented("CHAIN ""LEDGER""")
            Case "D" : GwRuntime.NotImplemented("CHAIN ""SALES""")
            Case "E" : GwRuntime.NotImplemented("CHAIN ""LOGBOOK""")
            Case "H" : GwRuntime.NotImplemented("GOTO 2300 subliminal")
            Case "J" : GwRuntime.NotImplemented("GOTO 1290 spool real names")
            Case "K" : GwRuntime.NotImplemented("SHELL ""BILL""")
            Case "L" : GwRuntime.NotImplemented("GOTO 1460 password")
            Case "M" : GwRuntime.NotImplemented("CHAIN ""QUOTE""")
            Case "N" : GwRuntime.NotImplemented("CHAIN ""PHONE""")
            Case "O" : GwRuntime.NotImplemented("GOTO 1590 copy spec index")
            Case "P" : GwRuntime.NotImplemented("CHAIN ""ENTIRE""")
            Case "Q", "q"
                GwRuntime.NotImplemented("Launch Word Processor (external)")
            Case "R"
                GwRuntime.NotImplemented("TS F:\WORD*.DOC search")
            Case "S"
                GwRuntime.NotImplemented("Keyboard clean / MESSAGE module")
            Case "T" : GwRuntime.NotImplemented("GOTO 1670 set time/date")
            Case "U" : GwRuntime.NotImplemented("SYSTEM / phone line")
            Case "V" : GwRuntime.NotImplemented("GOTO 2080 change menu message")
            Case "W" : GwRuntime.NotImplemented("CHAIN ""PHMESAGE""")
            Case "X" : GwRuntime.NotImplemented("GOTO 2150 typewriter mode")
            Case "Y" : GwRuntime.NotImplemented("CHAIN ""EDBACKUP""")
            Case "Z" : GwRuntime.NotImplemented("PMS /D /B personal calendar")
            Case "1" : GwRuntime.NotImplemented("CHAIN ""MILEAGE""")
            Case "2" : GwRuntime.NotImplemented("CHAIN ""purchase""")
            Case "3" : GwRuntime.NotImplemented("SHELL menu menu")
            Case "4" : GwRuntime.NotImplemented("CHAIN ""logenter""")
            Case "5" : GwRuntime.NotImplemented("CHAIN ""VGAFONTS""")
            Case "6" : GwRuntime.NotImplemented("QB /RUN CADMIUM")
            Case "7" : GwRuntime.NotImplemented("PAYROLL system")
            Case Else
                ' ignore unknown key
        End Select
    End Sub

    ' (Lines 2710..2810)
    Private Sub MainMenu_2710_CalcDayOfWeek()
        ' Your GWBASIC code computes weekday from DATE$ (mm-dd-yyyy).
        ' In VB.NET, we can compute directly but keep TODAY$ output same.
        TODAY$ = DateTime.Now.DayOfWeek.ToString()
        ' GWBASIC uses "Sunday" etc, this matches.
    End Sub

End Class