' ... inside FormMainMenu class ...

' UI controls
Private rtbScreen As New RichTextBox()

Public Sub New()
    Me.Text = "MAINMENU (GWBASIC port)"
    Me.Width = 1000
    Me.Height = 700

    rtbScreen.Dock = DockStyle.Fill
    rtbScreen.Font = New Drawing.Font("Consolas", 12.0F)
    rtbScreen.ReadOnly = True
    rtbScreen.Multiline = True
    rtbScreen.BackColor = Drawing.Color.Black
    rtbScreen.ForeColor = Drawing.Color.LightGray
    rtbScreen.ScrollBars = RichTextBoxScrollBars.Vertical

    Me.Controls.Add(rtbScreen)

    scr = New GwScreen(rtbScreen, cols:=80, rows:=25)
    tmr.Interval = 50
End Sub