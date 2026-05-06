Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms

Public Class FormPersonalCalendar

    ' Keep data under the share (per your instruction)
    Private Const DataDir As String = "\\invoice\MainMenu\Data"

    Private ReadOnly DatesPmsPath As String = Path.Combine(DataDir, "DATES.PMS")
    Private ReadOnly PmsDatPath As String = Path.Combine(DataDir, "PMS.DAT")
    Private ReadOnly PmsNumPath As String = Path.Combine(DataDir, "PMS.NUM")

    Private _cal As MonthCalendar = Nothing
    Private _grid As DataGridView = Nothing
    Private _lblStatus As Label = Nothing

    Private _entries As List(Of CalendarEntry) = New List(Of CalendarEntry)()

    Public Sub New()
        InitializeComponent()
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        Me.Text = "Personal Calendar"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.KeyPreview = True
        Me.MinimumSize = New Size(900, 560)

        If Not EnsureDataDirExistsFriendly() Then
            Me.Close()
            Return
        End If

        EnsurePmsFilesExist()

        BuildUi()
        LoadAllData()
        ApplyBoldedDates()
        JumpToLastDateIfAvailable()
        RefreshSelectedDateGrid()
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)

        If e.KeyCode = Keys.Escape Then
            Me.Close()
            e.Handled = True
            Return
        End If

        If e.Control AndAlso e.KeyCode = Keys.R Then
            LoadAllData()
            ApplyBoldedDates()
            RefreshSelectedDateGrid()
            e.Handled = True
            Return
        End If
    End Sub

    ' === UI ===
    Private Sub BuildUi()
        Dim root As New TableLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(12),
            .ColumnCount = 1,
            .RowCount = 3
        }
        root.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        root.RowStyles.Add(New RowStyle(SizeType.AutoSize))

        Dim lblHeader As New Label() With {
            .AutoSize = True,
            .Font = New Font("Segoe UI", 16.0F, FontStyle.Bold),
            .Text = "Personal Calendar"
        }

        Dim body As New TableLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 1
        }
        body.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        body.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        body.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))

        _cal = New MonthCalendar() With {
            .MaxSelectionCount = 1
        }
        AddHandler _cal.DateSelected, Sub() RefreshSelectedDateGrid()
        AddHandler _cal.DateChanged, Sub() RefreshSelectedDateGrid()

        _grid = New DataGridView() With {
            .Dock = DockStyle.Fill,
            .ReadOnly = True,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .MultiSelect = False,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        }

        _grid.Columns.Clear()
        _grid.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Date", .HeaderText = "Date"})
        _grid.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Text", .HeaderText = "Text"})

        _grid.Columns("Date").Width = 110
        _grid.Columns("Date").AutoSizeMode = DataGridViewAutoSizeColumnMode.None

        body.Controls.Add(_cal, 0, 0)
        body.Controls.Add(_grid, 1, 0)

        Dim footer As New TableLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 3,
            .RowCount = 1
        }
        footer.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        footer.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        footer.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))

        _lblStatus = New Label() With {
            .AutoSize = True,
            .Text = "Ctrl+R = Reload   Esc = Close"
        }

        Dim btnReload As New Button() With {.Text = "Reload (Ctrl+R)", .AutoSize = True}
        AddHandler btnReload.Click,
            Sub()
                LoadAllData()
                ApplyBoldedDates()
                RefreshSelectedDateGrid()
            End Sub

        Dim btnClose As New Button() With {.Text = "Close (Esc)", .AutoSize = True}
        AddHandler btnClose.Click, Sub() Me.Close()

        footer.Controls.Add(_lblStatus, 0, 0)
        footer.Controls.Add(btnReload, 1, 0)
        footer.Controls.Add(btnClose, 2, 0)

        root.Controls.Add(lblHeader, 0, 0)
        root.Controls.Add(body, 0, 1)
        root.Controls.Add(footer, 0, 2)

        Me.Controls.Clear()
        Me.Controls.Add(root)
    End Sub

    ' === Data model ===
    Private NotInheritable Class CalendarEntry
        Public Property [Date] As DateTime
        Public Property Text As String
    End Class

    ' === File/folder checks ===
    Private Function EnsureDataDirExistsFriendly() As Boolean
        Try
            If Directory.Exists(DataDir) Then Return True

            MessageBox.Show("Personal Calendar cannot open because the data folder is not available:" & Environment.NewLine &
                            DataDir & Environment.NewLine & Environment.NewLine &
                            "Please run on the VM or connect to the share.",
                            "Personal Calendar",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error)
            Return False
        Catch ex As Exception
            MessageBox.Show("Personal Calendar cannot access the data folder:" & Environment.NewLine &
                            DataDir & Environment.NewLine & Environment.NewLine &
                            ex.Message,
                            "Personal Calendar",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Private Sub EnsurePmsFilesExist()
        Try
            If Not File.Exists(DatesPmsPath) Then
                ' Minimal header line similar to what you showed (mask + 1,31).
                ' PMS likely expects it, so we keep it.
                File.WriteAllText(DatesPmsPath,
                                  ToBasicWriteLine(New String() {New String("1"c, 60), "1", "31"}) & Environment.NewLine,
                                  Encoding.ASCII)
            End If

            If Not File.Exists(PmsDatPath) Then
                File.WriteAllText(PmsDatPath, "", Encoding.ASCII)
            End If

            If Not File.Exists(PmsNumPath) Then
                ' Default: flag 0, lastDate = today, count 0
                File.WriteAllText(PmsNumPath,
                                  ToBasicWriteLine(New String() {"0", DateTime.Today.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture), "0"}) & Environment.NewLine,
                                  Encoding.ASCII)
            End If
        Catch ex As Exception
            MessageBox.Show("Could not create/open PMS files in:" & Environment.NewLine &
                            DataDir & Environment.NewLine & Environment.NewLine &
                            ex.Message,
                            "Personal Calendar",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error)
            Me.Close()
        End Try
    End Sub

    ' === Load/parse ===
    Private Sub LoadAllData()
        _entries.Clear()

        ' Primary events file is DATES.PMS (based on your sample: "MM-dd-yyyy","text")
        LoadDatesPmsEntries()

        _lblStatus.Text = $"Loaded entries: {_entries.Count}    Ctrl+R = Reload   Esc = Close"
    End Sub

    Private Sub LoadDatesPmsEntries()
        If Not File.Exists(DatesPmsPath) Then Return

        Dim lines = File.ReadAllLines(DatesPmsPath, Encoding.ASCII).
            Select(Function(l) StripDosEofAndControls(l).Trim()).
            Where(Function(l) l.Length > 0).
            ToList()

        If lines.Count = 0 Then Return

        Dim startIndex As Integer = 0

        ' If first line looks like the header: 3 fields (string, number, number) and the string is all 1s
        Dim headerParts = ParseBasicWriteLine(lines(0))
        If headerParts.Count >= 3 Then
            Dim mask = headerParts(0)
            If Not String.IsNullOrEmpty(mask) AndAlso mask.All(Function(ch) ch = "1"c) Then
                startIndex = 1
            End If
        End If

        For i As Integer = startIndex To lines.Count - 1
            Dim parts = ParseBasicWriteLine(lines(i))
            If parts.Count < 2 Then Continue For

            Dim dtText = parts(0)
            Dim txt = parts(1)

            Dim dt As DateTime
            If DateTime.TryParseExact(dtText, "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, dt) Then
                _entries.Add(New CalendarEntry With {
                    .Date = dt.Date,
                    .Text = If(txt, "").Trim()
                })
            End If
        Next
    End Sub

    Private Sub ApplyBoldedDates()
        If _cal Is Nothing Then Return

        ' MonthCalendar only supports "bolded dates" by giving it an array.
        ' We'll bold every date that has at least one entry.
        Dim boldDates = _entries.
            Select(Function(e) e.Date).
            Distinct().
            OrderBy(Function(d) d).
            ToArray()

        _cal.BoldedDates = boldDates
        _cal.UpdateBoldedDates()
    End Sub

    Private Sub JumpToLastDateIfAvailable()
        Try
            If Not File.Exists(PmsNumPath) Then Return

            Dim raw = StripDosEofAndControls(File.ReadAllText(PmsNumPath, Encoding.ASCII)).Trim()
            If raw.Length = 0 Then Return

            ' Only care about second field: "MM-dd-yyyy"
            Dim parts = ParseBasicWriteLine(raw)
            If parts.Count < 2 Then Return

            Dim dtText = parts(1)
            Dim dt As DateTime
            If DateTime.TryParseExact(dtText, "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, dt) Then
                _cal.SetDate(dt.Date)
            End If
        Catch
            ' Non-fatal: ignore and just show today
        End Try
    End Sub

    Private Sub RefreshSelectedDateGrid()
        If _grid Is Nothing OrElse _cal Is Nothing Then Return

        Dim selected = _cal.SelectionStart.Date

        Dim todays = _entries.
            Where(Function(e) e.Date = selected).
            OrderBy(Function(e) e.Text).
            ToList()

        _grid.Rows.Clear()
        For Each e In todays
            _grid.Rows.Add(e.Date.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture), e.Text)
        Next
    End Sub

    ' === BASIC WRITE# compatibility helpers (same idea as Mileage module) ===
    Private Shared Function StripDosEofAndControls(s As String) As String
        If s Is Nothing Then Return ""
        Dim sb As New StringBuilder(s.Length)
        For Each ch As Char In s
            If Char.IsControl(ch) Then Continue For
            sb.Append(ch)
        Next
        Return sb.ToString()
    End Function

    Private Shared Function ToBasicWriteLine(values As IEnumerable(Of String)) As String
        Dim parts As New List(Of String)()
        For Each v In values
            If v Is Nothing Then
                parts.Add("""""")
            Else
                Dim n As Integer
                If Integer.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, n) AndAlso
                   v.All(Function(ch) Char.IsDigit(ch) OrElse ch = "-"c) Then
                    parts.Add(n.ToString(CultureInfo.InvariantCulture))
                Else
                    Dim escaped = v.Replace("""", """""")
                    parts.Add("""" & escaped & """")
                End If
            End If
        Next
        Return String.Join(",", parts)
    End Function

    Private Shared Function ParseBasicWriteLine(line As String) As List(Of String)
        Dim result As New List(Of String)()
        Dim sb As New StringBuilder()
        Dim inQuotes As Boolean = False
        Dim i As Integer = 0

        While i < line.Length
            Dim ch = line(i)

            If inQuotes Then
                If ch = """"c Then
                    If i + 1 < line.Length AndAlso line(i + 1) = """"c Then
                        sb.Append(""""c)
                        i += 2
                    Else
                        inQuotes = False
                        i += 1
                    End If
                Else
                    sb.Append(ch)
                    i += 1
                End If
            Else
                If ch = ","c Then
                    result.Add(sb.ToString().Trim())
                    sb.Clear()
                    i += 1
                ElseIf ch = """"c Then
                    inQuotes = True
                    i += 1
                Else
                    sb.Append(ch)
                    i += 1
                End If
            End If
        End While

        result.Add(sb.ToString().Trim())
        Return result
    End Function

End Class