Option Strict Off
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms

Public Class DosMenuFormBase
    Inherits Form

    Protected ReadOnly lblMainMenu As New Label()
    Protected ReadOnly lblDateTime As New Label()
    Protected ReadOnly tmrClock As New Timer()

    Protected ReadOnly flpLeft As New FlowLayoutPanel()
    Protected ReadOnly flpRight As New FlowLayoutPanel()

    Private _displayVersion As String = ""
    Private _keyHandlers As New Dictionary(Of String, Action)(StringComparer.OrdinalIgnoreCase)

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        Me.KeyPreview = True
        Me.BackColor = Color.Black
        Me.ForeColor = Color.White
        Me.StartPosition = FormStartPosition.CenterParent

        _displayVersion = ""
        Try
            _displayVersion = BuildInfo.DisplayVersion
        Catch
            _displayVersion = ""
        End Try

        BuildLayout()
        UpdateHeaderClock()

        tmrClock.Interval = 1000
        AddHandler tmrClock.Tick, Sub() UpdateHeaderClock()
        tmrClock.Start()

        AddHandler Me.KeyPress, AddressOf OnBaseKeyPress
        AddHandler flpLeft.SizeChanged, Sub() ResizeButtonsToPanel(flpLeft)
        AddHandler flpRight.SizeChanged, Sub() ResizeButtonsToPanel(flpRight)

        Me.BeginInvoke(New Action(Sub()
                                      ResizeButtonsToPanel(flpLeft)
                                      ResizeButtonsToPanel(flpRight)
                                  End Sub))
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        MyBase.OnFormClosed(e)
        Try
            tmrClock.Stop()
        Catch
        End Try
    End Sub

    Private Sub BuildLayout()
        Dim root As New TableLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 3,
            .BackColor = Color.Black,
            .Padding = New Padding(12)
        }
        root.RowStyles.Add(New RowStyle(SizeType.AutoSize)) ' title
        root.RowStyles.Add(New RowStyle(SizeType.AutoSize)) ' date/time
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F)) ' body

        lblMainMenu.AutoSize = False
        lblMainMenu.Height = 44
        lblMainMenu.Dock = DockStyle.Fill
        lblMainMenu.TextAlign = ContentAlignment.MiddleLeft
        lblMainMenu.Font = New Font("Segoe UI", 22.0F, FontStyle.Bold, GraphicsUnit.Point)
        lblMainMenu.ForeColor = Color.Gold
        lblMainMenu.BackColor = Color.Black

        lblDateTime.AutoSize = False
        lblDateTime.Height = 34
        lblDateTime.Dock = DockStyle.Fill
        lblDateTime.TextAlign = ContentAlignment.MiddleLeft
        lblDateTime.Font = New Font("Segoe UI", 12.0F, FontStyle.Regular, GraphicsUnit.Point)
        lblDateTime.ForeColor = Color.White
        lblDateTime.BackColor = Color.Black
        lblDateTime.Padding = New Padding(0, 10, 0, 0)
        lblDateTime.Margin = New Padding(0)

        Dim body As New TableLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 1,
            .BackColor = Color.Black
        }
        body.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        body.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))

        ConfigureMenuPanel(flpLeft)
        ConfigureMenuPanel(flpRight)

        body.Controls.Add(flpLeft, 0, 0)
        body.Controls.Add(flpRight, 1, 0)

        root.Controls.Add(lblMainMenu, 0, 0)
        root.Controls.Add(lblDateTime, 0, 1)
        root.Controls.Add(body, 0, 2)

        Controls.Clear()
        Controls.Add(root)
    End Sub

    Private Shared Sub ConfigureMenuPanel(panel As FlowLayoutPanel)
        panel.Dock = DockStyle.Fill
        panel.FlowDirection = FlowDirection.TopDown
        panel.WrapContents = False
        panel.AutoScroll = True
        panel.BackColor = Color.Black
        panel.Padding = New Padding(0)
        panel.Margin = New Padding(0, 6, 0, 0)
    End Sub

    Protected Sub SetMenuTitle(title As String)
        lblMainMenu.Text = title
        Me.Text = title
    End Sub

    Protected Sub ClearMenu()
        flpLeft.Controls.Clear()
        flpRight.Controls.Clear()
        _keyHandlers = New Dictionary(Of String, Action)(StringComparer.OrdinalIgnoreCase)
    End Sub

    Protected Sub AddMenuButton(panel As FlowLayoutPanel, key As String, text As String, handler As Action)
        Dim btn As New Button()

        btn.AutoSize = False
        btn.Height = 36
        btn.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btn.TextAlign = ContentAlignment.MiddleLeft
        btn.Text = "(" & key & ") " & text
        btn.Tag = key
        btn.Margin = New Padding(3, 3, 3, 6)

        btn.UseVisualStyleBackColor = False
        btn.BackColor = Color.Black
        btn.ForeColor = Color.White
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderColor = Color.DimGray
        btn.FlatAppearance.BorderSize = 1
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(32, 32, 32)
        btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(64, 64, 64)
        btn.AutoEllipsis = True

        AddHandler btn.Click, Sub() handler()

        panel.Controls.Add(btn)

        If Not String.IsNullOrWhiteSpace(key) Then
            _keyHandlers(key) = handler
        End If
    End Sub

    Private Sub ResizeButtonsToPanel(panel As FlowLayoutPanel)
        If panel Is Nothing Then Return

        Dim targetWidth As Integer =
            panel.ClientSize.Width -
            panel.Padding.Left - panel.Padding.Right -
            SystemInformation.VerticalScrollBarWidth - 6

        If targetWidth < 150 Then targetWidth = 150

        For Each c As Control In panel.Controls
            Dim btn = TryCast(c, Button)
            If btn IsNot Nothing Then btn.Width = targetWidth
        Next
    End Sub

    Private Sub OnBaseKeyPress(sender As Object, e As KeyPressEventArgs)
        Dim ch As String = e.KeyChar.ToString()
        If ch = vbCr OrElse ch = vbLf Then Return

        Dim up As String = ch
        If up.Length = 1 AndAlso Char.IsLetter(up(0)) Then up = up.ToUpperInvariant()

        Dim act As Action = Nothing
        If _keyHandlers IsNot Nothing AndAlso _keyHandlers.TryGetValue(up, act) Then
            act()
        End If
    End Sub

    Protected Sub UpdateHeaderClock()
        Dim now As DateTime = DateTime.Now
        Dim ver As String = If(String.IsNullOrWhiteSpace(_displayVersion), "", "   " & _displayVersion)

        lblDateTime.Text = ver & "          " &
                           now.ToString("dddd") & "  " &
                           now.ToString("MM-dd-yyyy") & "          " &
                           now.ToString("hh:mm:ss tt")
    End Sub

    Protected Sub NotYet(feature As String)
        MessageBox.Show("Not implemented yet: " & feature, "Port status", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

End Class