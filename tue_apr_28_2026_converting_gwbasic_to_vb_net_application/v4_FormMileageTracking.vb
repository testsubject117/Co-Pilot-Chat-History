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

    Private ReadOnly _filePath As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "AMiOffice", "mileage.csv")

    Private _table As DataTable = Nothing!
    Private _grid As DataGridView = Nothing!

    ' Inputs
    Private _dtpDate As DateTimePicker = Nothing!
    Private _txtStart As TextBox = Nothing!
    Private _txtEnd As TextBox = Nothing!
    Private _txtPurpose As TextBox = Nothing!
    Private _lblMiles As Label = Nothing!

    Public Sub New()
        InitializeComponent()
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        Me.Text = "Mileage Tracking"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.KeyPreview = True
        Me.MinimumSize = New Size(950, 620)

        BuildUi()
        BuildTable()
        LoadFromDisk()
        RecalcMilesPreview()
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)

        If e.Control AndAlso e.KeyCode = Keys.S Then
            SaveToDisk()
            e.Handled = True
            Return
        End If

        If e.KeyCode = Keys.Escape Then
            Close()
            e.Handled = True
            Return
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

        ' Entry panel
        Dim entry As New TableLayoutPanel() With {
            .Dock = DockStyle.Top,
            .AutoSize = True,
            .ColumnCount = 9
        }
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))         ' Date label
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 140))    ' Date picker
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))         ' Start label
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90))     ' Start
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))         ' End label
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90))     ' End
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))         ' Miles preview
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))  ' Purpose
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))         ' Buttons

        _dtpDate = New DateTimePicker() With {
            .Format = DateTimePickerFormat.Short,
            .Value = DateTime.Today,
            .Dock = DockStyle.Fill
        }

        _txtStart = New TextBox() With {.Dock = DockStyle.Fill}
        _txtEnd = New TextBox() With {.Dock = DockStyle.Fill}
        _txtPurpose = New TextBox() With {.Dock = DockStyle.Fill}

        AddHandler _txtStart.TextChanged, Sub() RecalcMilesPreview()
        AddHandler _txtEnd.TextChanged, Sub() RecalcMilesPreview()
        AddHandler _txtPurpose.KeyDown,
            Sub(sender, e)
                If e.KeyCode = Keys.Enter Then
                    AddEntry()
                    e.SuppressKeyPress = True
                End If
            End Sub

        _lblMiles = New Label() With {
            .AutoSize = True,
            .Font = New Font("Segoe UI", 10.0F, FontStyle.Bold),
            .Text = "Miles: —",
            .Padding = New Padding(8, 7, 8, 7)
        }

        Dim btnAdd As New Button() With {.Text = "Add", .AutoSize = True}
        AddHandler btnAdd.Click, Sub() AddEntry()

        Dim btnDelete As New Button() With {.Text = "Delete Selected", .AutoSize = True}
        AddHandler btnDelete.Click, Sub() DeleteSelected()

        Dim btnSave As New Button() With {.Text = "Save (Ctrl+S)", .AutoSize = True}
        AddHandler btnSave.Click, Sub() SaveToDisk()

        Dim btnClose As New Button() With {.Text = "(Esc) Close", .AutoSize = True}
        AddHandler btnClose.Click, Sub() Close()

        Dim rightButtons As New FlowLayoutPanel() With {
            .AutoSize = True,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False,
            .Margin = New Padding(10, 0, 0, 0)
        }
        rightButtons.Controls.Add(btnAdd)
        rightButtons.Controls.Add(btnDelete)
        rightButtons.Controls.Add(btnSave)
        rightButtons.Controls.Add(btnClose)

        entry.Controls.Add(New Label() With {.AutoSize = True, .Text = "Date:", .Padding = New Padding(0, 8, 6, 0)}, 0, 0)
        entry.Controls.Add(_dtpDate, 1, 0)
        entry.Controls.Add(New Label() With {.AutoSize = True, .Text = "Start:", .Padding = New Padding(12, 8, 6, 0)}, 2, 0)
        entry.Controls.Add(_txtStart, 3, 0)
        entry.Controls.Add(New Label() With {.AutoSize = True, .Text = "End:", .Padding = New Padding(12, 8, 6, 0)}, 4, 0)
        entry.Controls.Add(_txtEnd, 5, 0)
        entry.Controls.Add(_lblMiles, 6, 0)
        entry.Controls.Add(_txtPurpose, 7, 0)
        entry.Controls.Add(rightButtons, 8, 0)

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
        root.Controls.Add(_grid, 0, 2)

        Controls.Clear()
        Controls.Add(root)
    End Sub

    Private Sub BuildTable()
        _table = New DataTable("Mileage")
        _table.Columns.Add("Date", GetType(DateTime))
        _table.Columns.Add("Start", GetType(Integer))
        _table.Columns.Add("End", GetType(Integer))
        _table.Columns.Add("Miles", GetType(Integer))
        _table.Columns.Add("Purpose", GetType(String))

        _grid.DataSource = _table

        _grid.Columns("Date").DefaultCellStyle.Format = "d"
        _grid.Columns("Start").FillWeight = 12
        _grid.Columns("End").FillWeight = 12
        _grid.Columns("Miles").FillWeight = 10
        _grid.Columns("Purpose").FillWeight = 66
    End Sub

    Private Sub RecalcMilesPreview()
        Dim startVal As Integer
        Dim endVal As Integer

        If Integer.TryParse(_txtStart.Text.Trim(), startVal) AndAlso Integer.TryParse(_txtEnd.Text.Trim(), endVal) Then
            Dim miles = endVal - startVal
            If miles >= 0 Then
                _lblMiles.Text = "Miles: " & miles.ToString(CultureInfo.InvariantCulture)
            Else
                _lblMiles.Text = "Miles: (end < start)"
            End If
        Else
            _lblMiles.Text = "Miles: —"
        End If
    End Sub

    Private Sub AddEntry()
        Dim startVal As Integer
        Dim endVal As Integer

        If Not Integer.TryParse(_txtStart.Text.Trim(), startVal) Then
            MessageBox.Show("Start must be a whole number (odometer).", "Mileage", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            _txtStart.Focus()
            Return
        End If
        If Not Integer.TryParse(_txtEnd.Text.Trim(), endVal) Then
            MessageBox.Show("End must be a whole number (odometer).", "Mileage", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            _txtEnd.Focus()
            Return
        End If

        Dim miles = endVal - startVal
        If miles < 0 Then
            MessageBox.Show("End cannot be less than Start.", "Mileage", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            _txtEnd.Focus()
            Return
        End If

        Dim purpose = _txtPurpose.Text.Trim()

        Dim row = _table.NewRow()
        row("Date") = _dtpDate.Value.Date
        row("Start") = startVal
        row("End") = endVal
        row("Miles") = miles
        row("Purpose") = purpose
        _table.Rows.Add(row)

        _txtStart.Clear()
        _txtEnd.Clear()
        _txtPurpose.Clear()
        _txtStart.Focus()
        RecalcMilesPreview()
    End Sub

    Private Sub DeleteSelected()
        If _grid.SelectedRows.Count = 0 Then Return

        If MessageBox.Show($"Delete {_grid.SelectedRows.Count} selected row(s)?",
                           "Mileage", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
            Return
        End If

        Dim idxs = _grid.SelectedRows.Cast(Of DataGridViewRow)().
            Select(Function(r) r.Index).
            OrderByDescending(Function(i) i).
            ToList()

        For Each i In idxs
            If i >= 0 AndAlso i < _table.Rows.Count Then
                _table.Rows.RemoveAt(i)
            End If
        Next
    End Sub

    Private Sub EnsureDir()
        Dim dir = Path.GetDirectoryName(_filePath)
        If Not String.IsNullOrWhiteSpace(dir) Then
            Directory.CreateDirectory(dir)
        End If
    End Sub

    Private Sub SaveToDisk()
        Try
            EnsureDir()

            Using sw As New StreamWriter(_filePath, append:=False, encoding:=Encoding.UTF8)
                sw.WriteLine("Date,Start,End,Miles,Purpose")
                For Each r As DataRow In _table.Rows
                    Dim d = DirectCast(r("Date"), DateTime).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    Dim s = CInt(r("Start")).ToString(CultureInfo.InvariantCulture)
                    Dim en = CInt(r("End")).ToString(CultureInfo.InvariantCulture)
                    Dim m = CInt(r("Miles")).ToString(CultureInfo.InvariantCulture)
                    Dim p = EscapeCsv(CStr(r("Purpose")))
                    sw.WriteLine($"{d},{s},{en},{m},{p}")
                Next
            End Using

            MessageBox.Show("Saved to:" & Environment.NewLine & _filePath,
                            "Mileage", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("Save failed: " & ex.Message, "Mileage", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LoadFromDisk()
        Try
            If Not File.Exists(_filePath) Then Return

            Dim lines = File.ReadAllLines(_filePath, Encoding.UTF8)
            If lines.Length <= 1 Then Return

            For i = 1 To lines.Length - 1
                Dim line = lines(i)
                If String.IsNullOrWhiteSpace(line) Then Continue For

                Dim parts = ParseCsvLine(line)
                If parts.Length < 5 Then Continue For

                Dim d As DateTime
                Dim s As Integer
                Dim en As Integer
                Dim m As Integer

                If Not DateTime.TryParseExact(parts(0), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, d) Then Continue For
                If Not Integer.TryParse(parts(1), s) Then Continue For
                If Not Integer.TryParse(parts(2), en) Then Continue For
                If Not Integer.TryParse(parts(3), m) Then Continue For

                Dim row = _table.NewRow()
                row("Date") = d.Date
                row("Start") = s
                row("End") = en
                row("Miles") = m
                row("Purpose") = parts(4)
                _table.Rows.Add(row)
            Next
        Catch ex As Exception
            MessageBox.Show("Load failed: " & ex.Message, "Mileage", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Shared Function EscapeCsv(value As String) As String
        If value Is Nothing Then Return ""
        Dim needsQuotes = value.Contains(","c) OrElse value.Contains(""""c) OrElse value.Contains(ControlChars.Cr) OrElse value.Contains(ControlChars.Lf)
        Dim v = value.Replace("""", """""")
        If needsQuotes Then Return """" & v & """"
        Return v
    End Function

    Private Shared Function ParseCsvLine(line As String) As String()
        Dim result As New List(Of String)()
        Dim sb As New StringBuilder()
        Dim inQuotes = False

        For i = 0 To line.Length - 1
            Dim ch = line(i)

            If inQuotes Then
                If ch = """"c Then
                    If i + 1 < line.Length AndAlso line(i + 1) = """"c Then
                        sb.Append(""""c)
                        i += 1
                    Else
                        inQuotes = False
                    End If
                Else
                    sb.Append(ch)
                End If
            Else
                If ch = ","c Then
                    result.Add(sb.ToString())
                    sb.Clear()
                ElseIf ch = """"c Then
                    inQuotes = True
                Else
                    sb.Append(ch)
                End If
            End If
        Next

        result.Add(sb.ToString())
        Return result.ToArray()
    End Function

End Class