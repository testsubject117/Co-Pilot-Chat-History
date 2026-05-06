Option Strict On
Option Explicit On

Imports System
Imports System.Windows.Forms

Public Class FormMileageTracking

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub FormMileageTracking_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Mileage Tracking"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.KeyPreview = True
    End Sub

    Private Sub FormMileageTracking_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.Escape Then
            Me.Close()
            e.Handled = True
        End If
    End Sub

End Class