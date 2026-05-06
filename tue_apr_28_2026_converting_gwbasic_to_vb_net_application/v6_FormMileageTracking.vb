Option Strict On
Option Explicit On

Imports System
Imports System.Data
Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms

Public Class FormMileageTracking

    ' Legacy file locations (per your requirement)
    Private Const DataDir As String = "\\invoice\data"
    Private ReadOnly MileageDatPath As String = Path.Combine(DataDir, "MILEAGE.DAT")
    Private ReadOnly OdometerNumPath As String = Path.Combine(DataDir, "ODOMETER.NUM")
    Private ReadOnly MileageTmpPath As String = Path.Combine(DataDir, "MILEAGE.TMP")

    Private _table As DataTable = Nothing
    Private _grid As DataGridView = Nothing

    ' Inputs
    Private _dtpDate As DateTimePicker = Nothing
    Private _txtDestination As TextBox = Nothing
    Private _txtPurpose As TextBox = Nothing
    Private _txtStart As TextBox = Nothing
    Private _txtEnd As TextBox = Nothing
    Private _txtMiles As TextBox = Nothing
    Private _cmbPayment As ComboBox = Nothing
    Private _lblMilesHint As Label = Nothing

    ' Buttons (so hotkeys can call PerformClick)
    Private _btnAdd As Button = Nothing
    Private _btnDelete As Button = Nothing
    Private _btnSave As Button = Nothing
    Private _btnReload As Button = Nothing
    Private _btnClose As Button = Nothing

    Private _lastOdometerDefault As Integer = 0

    Public Sub New()
        InitializeComponent()
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        Me.Text = "Mileage Tracking"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.KeyPreview = True
        Me.MinimumSize = New Size(1050, 680)

        EnsureDataDirExists()

        BuildUi()
        BuildTable()

        _lastOdometerDefault = LoadLastOdometer()
        ApplyDefaultStartOdometer()

        LoadFromLegacyDat()

        RecalcMilesHint()
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)

        If e.Control AndAlso e.KeyCode = Keys.S Then
            SaveAllToLegacyDat()
            e.Handled = True
            Return
        End If

        If e.KeyCode = Keys.Escape Then
            Close()
            e.Handled = True
            Return
        End If
    End Sub

    Private Sub EnsureDataDirExists()
        ' UNC share should already exist, but this avoids crashes if it’s missing.
        If Not Directory.Exists(DataDir) Then
            Throw New DirectoryNotFoundException("Data directory not found: " & DataDir)
        End If
    End Sub

    Private Sub BuildUi()
        Dim root As New TableLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(12),
            .ColumnCount = 1,
            .RowCount = 3
        }
        root.RowStyles.Add(New RowStyle(SizeType.AutoSize))          ' header
        root.RowStyles.Add(New RowStyle(SizeType.AutoSize))          ' entry
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))   ' grid

        Dim lblHeader As New Label() With {
            .AutoSize = True,
            .Font = New Font("Segoe UI", 16.0F, FontStyle.Bold),
            .Text = "Mileage Tracking"
        }

        ' Entry panel (Add Mileage)
        Dim entry As New TableLayoutPanel() With {
            .Dock = DockStyle.Top,
            .AutoSize = True,
            .ColumnCount = 10
        }

        entry.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))        ' Date
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 140))
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))        ' Destination
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220))
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))        ' Purpose
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))        ' Start
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90))
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))        ' End
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90))

        _dtpDate = New DateTimePicker() With {
            .Format = DateTimePickerFormat.Short,
            .Value = DateTime.Today,
            .Dock = DockStyle.Fill
        }

        _txtDestination = New TextBox() With {.Dock = DockStyle.Fill}
        _txtPurpose = New TextBox() With {.Dock = DockStyle.Fill}
        _txtStart = New TextBox() With {.Dock = DockStyle.Fill}
        _txtEnd = New TextBox() With {.Dock = DockStyle.Fill}

        AddHandler _txtStart.TextChanged, Sub() SyncMilesDefault()
        AddHandler _txtEnd.TextChanged, Sub() SyncMilesDefault()

        entry.Controls.Add(New Label() With {.AutoSize = True, .Text = "Date:", .Padding = New Padding(0, 8, 6, 0)}, 0, 0)
        entry.Controls.Add(_dtpDate, 1, 0)
        entry.Controls.Add(New Label() With {.AutoSize = True, .Text = "Destination:", .Padding = New Padding(12, 8, 6, 0)}, 2, 0)
        entry.Controls.Add(_txtDestination, 3, 0)
        entry.Controls.Add(New Label() With {.AutoSize = True, .Text = "Business Purpose:", .Padding = New Padding(12, 8, 6, 0)}, 4, 0)
        entry.Controls.Add(_txtPurpose, 5, 0)
        entry.Controls.Add(New Label() With {.AutoSize = True, .Text = "Start:", .Padding = New Padding(12, 8, 6, 0)}, 6, 0)
        entry.Controls.Add(_txtStart, 7, 0)
        entry.Controls.Add(New Label() With {.AutoSize = True, .Text = "End:", .Padding = New Padding(12, 8, 6, 0)}, 8, 0)
        entry.Controls.Add(_txtEnd, 9, 0)

        ' Second row: miles + payment + buttons
        Dim entry2 As New TableLayoutPanel() With {
            .Dock = DockStyle.Top,
            .AutoSize = True,
            .ColumnCount = 8
        }
        entry2.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))       ' Miles
        entry2.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90))
        entry2.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))       ' hint
        entry2.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))       ' Payment
        entry2.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120))
        entry2.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F)) ' spacer
        entry2.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))       ' buttons
        entry2.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))       ' close spacer

        _txtMiles = New TextBox() With {.Dock = DockStyle.Fill}
        _lblMilesHint = New Label() With {
            .AutoSize = True,
            .Font = New Font("Segoe UI", 10.0F, FontStyle.Bold),
            .Text = "Miles: —",
            .Padding = New Padding(8, 7, 8, 7)
        }

        _cmbPayment = New ComboBox() With {
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Dock = DockStyle.Fill
        }
        _cmbPayment.Items.Add("Cash")
        _cmbPayment.Items.Add("Charge")
        _cmbPayment.SelectedIndex = 0

        _btnAdd = New Button() With {.Text = "Add", .AutoSize = True}
        AddHandler _btnAdd.Click, Sub() AddEntry()

        _btnDelete = New Button() With {.Text = "Delete Selected", .AutoSize = True}
        AddHandler _btnDelete.Click, Sub() DeleteSelected()

        _btnSave = New Button() With {.Text = "Save (Ctrl+S)", .AutoSize = True}
        AddHandler _btnSave.Click, Sub() SaveAllToLegacyDat()

        _btnReload = New Button() With {.Text = "Reload", .AutoSize = True}
        AddHandler _btnReload.Click, Sub() ReloadFromDisk()

        _btnClose = New Button() With {.Text = "(Esc) Close", .AutoSize = True}
        AddHandler _btnClose.Click, Sub() Close()

        Dim btnPanel As New FlowLayoutPanel() With {
            .AutoSize = True,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False
        }
        btnPanel.Controls.Add(_btnAdd)
        btnPanel.Controls.Add(_btnDelete)
        btnPanel.Controls.Add(_btnSave)
        btnPanel.Controls.Add(_btnReload)
        btnPanel.Controls.Add(_btnClose)

        AddHandler _txtPurpose.KeyDown,
            Sub(sender, e)
                If e.KeyCode = Keys.Enter Then
                    AddEntry()
                    e.SuppressKeyPress = True
                End If
            End Sub

        entry2.Controls.Add(New Label() With {.AutoSize = True, .Text = "Business Miles:", .Padding = New Padding(0, 8, 6, 0)}, 0, 0)
        entry2.Controls.Add(_txtMiles, 1, 0)
        entry2.Controls.Add(_lblMilesHint, 2, 0)
        entry2.Controls.Add(New Label() With {.AutoSize = True, .Text = "Payment:", .Padding = New Padding(12, 8, 6, 0)}, 3, 0)
        entry2.Controls.Add(_cmbPayment, 4, 0)
        entry2.Controls.Add(New Panel() With {.Dock = DockStyle.Fill}, 5, 0)
        entry2.Controls.Add(btnPanel, 6, 0)

        _grid = New DataGridView() With {
            .Dock = DockStyle.Fill,
            .ReadOnly = False,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .MultiSelect = True,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        }

        root.Controls.Add(lblHeader, 0, 0)
        root.Controls.Add(entry, 0, 1)
        root.Controls.Add(entry2, 0, 2)

        ' Put grid under the entry panels
        Dim host As New TableLayoutPanel() With {.Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 2}
        host.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        host.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        host.Controls.Add(root, 0, 0)
        host.Controls.Add(_grid, 0, 1)

        Controls.Clear()
        Controls.Add(host)
    End Sub

    Private Sub BuildTable()
        _table = New DataTable("Mileage")

        _table.Columns.Add("Date", GetType(DateTime))
        _table.Columns.Add("Destination", GetType(String))
        _table.Columns.Add("BusinessPurpose", GetType(String))
        _table.Columns.Add("OdometerStart", GetType(Integer))
        _table.Columns.Add("OdometerEnd", GetType(Integer))
        _table.Columns.Add("BusinessMiles", GetType(Integer))
        _table.Columns.Add("PaymentType", GetType(String))

        _grid.DataSource = _table

        _grid.Columns("Date").DefaultCellStyle.Format = "d"
        _grid.Columns("OdometerStart").FillWeight = 12
        _grid.Columns("OdometerEnd").FillWeight = 12
        _grid.Columns("BusinessMiles").FillWeight = 10
        _grid.Columns("PaymentType").FillWeight = 10
        _grid.Columns("Destination").FillWeight = 22
        _grid.Columns("BusinessPurpose").FillWeight = 34
    End Sub

    Private Sub ApplyDefaultStartOdometer()
        If _lastOdometerDefault > 0 Then
            _txtStart.Text = _lastOdometerDefault.ToString(CultureInfo.InvariantCulture)
            _txtEnd.Focus()
        Else
            _txtStart.Clear()
        End If
    End Sub

    Private Sub SyncMilesDefault()
        RecalcMilesHint()

        Dim startVal As Integer
        Dim endVal As Integer
        If Integer.TryParse(_txtStart.Text.Trim(), startVal) AndAlso Integer.TryParse(_txtEnd.Text.Trim(), endVal) Then
            Dim miles = endVal - startVal
            If miles > 0 Then
                ' Only auto-fill miles when empty (so user can override)
                If String.IsNullOrWhiteSpace(_txtMiles.Text) Then
                    _txtMiles.Text = miles.ToString(CultureInfo.InvariantCulture)
                End If
            End If
        End If
    End Sub

    Private Sub RecalcMilesHint()
        Dim startVal As Integer
        Dim endVal As Integer

        If Integer.TryParse(_txtStart.Text.Trim(), startVal) AndAlso Integer.TryParse(_txtEnd.Text.Trim(), endVal) Then
            Dim miles = endVal - startVal
            If miles >= 0 Then
                _lblMilesHint.Text = "Miles: " & miles.ToString(CultureInfo.InvariantCulture)
            Else
                _lblMilesHint.Text = "Miles: (end < start)"
            End If
        Else
            _lblMilesHint.Text = "Miles: —"
        End If
    End Sub

    Private Sub AddEntry()
        Dim dest = _txtDestination.Text.Trim()
        Dim purpose = _txtPurpose.Text.Trim()

        Dim os As Integer
        Dim oe As Integer
        Dim bm As Integer

        If Not Integer.TryParse(_txtStart.Text.Trim(), os) Then
            MessageBox.Show("Odometer Start must be a whole number.", "Mileage", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            _txtStart.Focus()
            Return
        End If

       