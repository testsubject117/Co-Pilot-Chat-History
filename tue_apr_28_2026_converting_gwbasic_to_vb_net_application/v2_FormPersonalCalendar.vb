Option Strict On
Option Explicit On

Imports System
Imports System.Drawing
Imports System.Windows.Forms

Public Class FormPersonalCalendar

    Public Sub New()
        InitializeComponent()
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        Me.Text = "Personal Calendar"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.KeyPreview = True
        Me.MinimumSize = New Size(640, 420)

        Dim pnl As New TableLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(16),
            .ColumnCount = 1,
            .RowCount = 4
        }
        pnl.RowStyles.Add(New RowStyle(SizeType.AutoSize)) ' header
        pnl.RowStyles.Add(New RowStyle(SizeType.AutoSize)) ' body
        pnl.RowStyles.Add(New RowStyle(SizeType.Percent, 100)) ' filler
        pnl.RowStyles.Add(New RowStyle(SizeType.AutoSize)) ' buttons

        Dim lblHeader As New Label() With {
            .AutoSize = True,
            .Font = New Font("Segoe UI", 16.0F, FontStyle.Bold),
            .Text = "Personal Calendar"
        }

        Dim lblBody As New Label() With {
            .AutoSize = True,
            .Font = New Font("Segoe UI", 11.0F, FontStyle.Regular),
            .Text = "This screen is ready to be built." & Environment.NewLine &
                    "Press ESC or click Close to return to the Main Menu."
        }

        Dim btnClose As New Button() With {
            .Text = "Close",
            .AutoSize = True,
            .Anchor = AnchorStyles.Right
        }
        AddHandler btnClose.Click, Sub() Me.Close()

        Dim buttonPanel As New FlowLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.RightToLeft,
            .AutoSize = True,
            .WrapContents = False
        }
        buttonPanel.Controls.Add(btnClose)

        pnl.Controls.Add(lblHeader, 0, 0)
        pnl.Controls.Add(lblBody, 0, 1)
        pnl.Controls.Add(New Panel() With {.Dock = DockStyle.Fill}, 0, 2)
        pnl.Controls.Add(buttonPanel, 0, 3)

        Me.Controls.Clear()
        Me.Controls.Add(pnl)
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        If e.KeyCode = Keys.Escape Then
            Me.Close()
            e.Handled = True
        End If
    End Sub

End Class