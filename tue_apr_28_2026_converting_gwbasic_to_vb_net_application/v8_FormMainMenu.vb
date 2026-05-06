Private Function AddMenuButton(parent As FlowLayoutPanel, key As String, caption As String) As Button
    Dim btn As New Button() With {
        .Text = $"({key}) {caption}",
        .Tag = key,
        .AutoSize = False
        ' ... your existing styling ...
    }

    AddHandler btn.Click, AddressOf MenuButton_Click
    parent.Controls.Add(btn)
    Return btn
End Function