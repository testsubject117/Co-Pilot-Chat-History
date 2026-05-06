Option Strict On
Option Explicit On

Imports System.Linq

Public Class FormOtherChecksView

    Private ReadOnly _all As New List(Of OtherChkEntry)()

    Private ReadOnly btnRefresh As New Button()
    Private ReadOnly dgv As New DataGridView()
    Private ReadOnly lblTotal As New Label()

    Public Sub New()
        InitializeComponent()

        Text = "Other Checks (OTHER.CHK)"
        Width = 1100
        Height = 750
        StartPosition = FormStartPosition.CenterParent

        BuildUi()
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        LoadData()
        BindGrid()
    End Sub

    Private Sub BuildUi()
        Dim top As New FlowLayoutPanel() With {.Dock = DockStyle.Top, .Height = 45, .Padding = New Padding(10)}

        btnRefresh.Text = "Refresh"
        btnRefresh.AutoSize = True
        top.Controls.Add(btnRefresh)

        lblTotal.AutoSize = True
        lblTotal.Padding = New Padding(15, 10, 0, 0)
        lblTotal.Text = "Total: $0.00"
        top.Controls.Add(lblTotal)

        dgv.Dock = DockStyle.Fill
        dgv.ReadOnly = True
        dgv.AllowUserToAddRows = False
        dgv.AllowUserToDeleteRows = False
        dgv.AutoGenerateColumns = True
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect

        Controls.Add(dgv)
        Controls.Add(top)

        AddHandler btnRefresh.Click,
            Sub()
                LoadData()
                BindGrid()
            End Sub
    End Sub

    Private Sub LoadData()
        Dim path = LegacyDataPaths.OtherChk

        _all.Clear()
        dgv.DataSource = Nothing
        lblTotal.Text = "Total: $0.00"

        If Not System.IO.File.Exists(path) Then
            lblTotal.Text = "OTHER.CHK was not found at: " & path
            MessageBox.Show(Me,
                            "OTHER.CHK was not found at:" & Environment.NewLine &
                            path,
                            "Other Checks",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error)
            Return
        End If

        Try
            _all.AddRange(OtherChkReader.ReadAll(path))
        Catch ex As Exception
            lblTotal.Text = "Unable to read OTHER.CHK from: " & path
            MessageBox.Show(Me,
                            "Unable to read OTHER.CHK from:" & Environment.NewLine &
                            path & Environment.NewLine & Environment.NewLine &
                            ex.Message,
                            "Other Checks",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BindGrid()
        dgv.DataSource = _all.ToList()
        Dim total As Decimal = _all.Sum(Function(x) x.Amount)
        lblTotal.Text = $"Total: {total:C}   (Checks: {_all.Count:N0})"
    End Sub

End Class